using StealAllTheCats.Common;
using StealAllTheCats.Common.Dtos;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Mappers;
using StealAllTheCats.Dtos.Requets;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Services.Interfaces;
using System.Linq.Expressions;

namespace StealAllTheCats.Services;

public class CatService(IApiClient apiClient, IConfiguration configuration, IGenericRepository<CatEntity> catRepository, IGenericRepository<TagEntity> tagRepository)
    : ICatService
{
    private readonly CatApiConfig _configuration = configuration.CatApiConfig();

    public async Task<Result<FetchCatsResult>> FetchCatsAsync(int limit = 25)
    {
        var apiCatsResult = await GetCatsAsync(limit);
        if (!apiCatsResult.Success)
            return Result<FetchCatsResult>.Fail(apiCatsResult.ErrorMessage!);

        var newCats = new List<CatEntity>();
        var duplicateCount = 0;

        foreach (var apiCat in apiCatsResult.Data!)
        {
            if (catRepository.ExistsAsync(c => c.CatId == apiCat.Id).Result)
            {
                duplicateCount++;
                continue;
            }

            var tags = await GetOrCreateTags(
                apiCat.Breeds
                    .SelectMany(b => b.Temperament?.Split(',') ?? [])
                    .Select(t => t.Trim())
                    .Distinct());

            newCats.Add(new CatEntity
            {
                CatId = apiCat.Id,
                Width = apiCat.Width,
                Height = apiCat.Height,
                ImageUrl = apiCat.Url,
                Tags = tags
            });
        }
        
        await catRepository.AddRangeAsync(newCats);
        await catRepository.SaveChangesAsync();

        return Result<FetchCatsResult>.Ok(new FetchCatsResult
        {
            NewCatsCount = newCats.Count,
            DuplicateCatsCount = duplicateCount,
            Cats = newCats.Select(c => new CatDto
            {
                Id = c.Id,
                CatId = c.CatId,
                Width = c.Width,
                Height = c.Height,
                ImageUrl = c.ImageUrl,
                Tags = c.Tags?.Select(t => t.Name) ?? []
            }).ToList()
        });
    }

    public async Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken)
    {
        Expression<Func<CatEntity, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            filter = c => c.Tags.Any(t => t.Name == request.Tag);
        }

        int skip = (request.Page - 1) * request.PageSize;
        int take = request.PageSize;

        var (cats, totalItems) = await catRepository.GetPaginatedAsync(skip, take, filter);

        var catDtos = cats.Select(CatMapper.ToDto).ToList();

        return Result<CatPaginatedResult>.Ok(new CatPaginatedResult
        {
            TotalItems = totalItems,
            Cats = catDtos
        });
    }


    public async Task<Result<CatDto?>> GetCatByIdAsync(int id, CancellationToken cancellationToken)
    {
        var cat = await catRepository.GetByIdAsync(id);

        if (cat == null)
            return Result<CatDto?>.Fail("Cat not found.");

        var catDto = CatMapper.ToDto(cat);
        return Result<CatDto?>.Ok(catDto);
    }

    private async Task<Result<List<ExternalCatApiResponse>>> GetCatsAsync(int limit)
    {
        return await apiClient.GetAsync<List<ExternalCatApiResponse>>($"images/search?limit={limit}&has_breeds=true&api_key={_configuration.ApiKey}");
    }

    private async Task<List<TagEntity>> GetOrCreateTags(IEnumerable<string> tagNames)
    {
        var existingTags = await tagRepository.FindAsync(tag => tagNames.Contains(tag.Name));

        var tagList = existingTags.ToList();

        var existingTagNames = tagList.Select(tag => tag.Name).ToList();
        var newTagNames = tagNames.Except(existingTagNames).ToList();

        var newTags = newTagNames.Select(name => new TagEntity { Name = name }).ToList();

        if (newTags.Any())
        {
            await tagRepository.AddRangeAsync(newTags);
            await tagRepository.SaveChangesAsync();
            tagList.AddRange(newTags);
        }

        return tagList;
    }
}