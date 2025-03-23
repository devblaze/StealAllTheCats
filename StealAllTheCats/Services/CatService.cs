using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Mapper;
using StealAllTheCats.Dtos.Requets;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Models;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Services;

public class CatService(ApplicationDbContext dbContext, IApiClient apiClient, IConfiguration configuration) : ICatService
{
    public async Task<Result<FetchCatsResult>> FetchCatsAsync(int limit = 25)
    {
        var apiCatsResult = await GetCatsAsync(limit);
        if (!apiCatsResult.Success)
            return Result<FetchCatsResult>.Fail(apiCatsResult.ErrorMessage!);

        var newCats = new List<CatEntity>();
        var duplicateCount = 0;

        foreach (var apiCat in apiCatsResult.Data!)
        {
            if (await dbContext.Cats.AnyAsync(c => c.CatId == apiCat.Id))
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

        await dbContext.Cats.AddRangeAsync(newCats);
        await dbContext.SaveChangesAsync();

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

    public async Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Cats.Include(c => c.Tags).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            query = query.Where(c => c.Tags.Any(t => t.Name == request.Tag));
        }

        var totalCats = await query.CountAsync(cancellationToken);
        var fetchedCats = await query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtoList = CatMapper.ToDtoList(fetchedCats);

        return Result<CatPaginatedResult>.Ok(new CatPaginatedResult
        {
            TotalItems = totalCats,
            Cats = dtoList
        });
    }

    public async Task<Result<CatDto?>> GetCatByIdAsync(int id, CancellationToken cancellationToken)
    {
        var cat = await dbContext.Cats.Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (cat == null)
            return Result<CatDto?>.Fail("Cat not found.");

        var catDto = CatMapper.ToDto(cat);
        return Result<CatDto?>.Ok(catDto);
    }

    public async Task<Result<List<ExternalCatApiResponse>>> GetCatsAsync(int limit)
    {
        return await apiClient.GetAsync<List<ExternalCatApiResponse>>($"images/search?limit={limit}&has_breeds=true&api_key={configuration["CatApi:ApiKey"]}");
    }

    private async Task<List<TagEntity>> GetOrCreateTags(IEnumerable<string> names)
    {
        var existingTags = await dbContext.Tags.Where(t => names.Contains(t.Name)).ToListAsync();
        var newTagNames = names.Except(existingTags.Select(t => t.Name)).ToList();

        var newTags = newTagNames.Select(name => new TagEntity { Name = name }).ToList();

        await dbContext.Tags.AddRangeAsync(newTags);
        await dbContext.SaveChangesAsync();

        existingTags.AddRange(newTags);
        return existingTags;
    }
}