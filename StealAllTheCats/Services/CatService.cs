using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Services.Interfaces;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class CatService(IApiClient apiClient, ApplicationDbContext db) : ICatService
{
    public async Task<Result<FetchCatsResult>> FetchCatsAsync(int limit = 25)
    {
        var apiCatResults = await GetCatsAsync(limit);
        if (!apiCatResults.Success || apiCatResults.Data is null)
            return Result<FetchCatsResult>.Fail(apiCatResults.ErrorMessage ??
                                                "Failed fetching cats from external API.");

        var existingCatIds = (await db.Cats.Select(c => c.CatId).ToListAsync()).ToHashSet();
        var existingTagsDict = await db.Tags.ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var newCats = new List<CatEntity>();
        int duplicateCount = 0;

        foreach (var apiCat in apiCatResults.Data)
        {
            if (existingCatIds.Contains(apiCat.Id))
            {
                duplicateCount++;
                continue; // Skip duplicates clearly.
            }

            var catImageResult = await GetCatImageAsync(apiCat.Url);
            if (!catImageResult.Success || catImageResult.Data is null)
                continue; // optionally log or handle image failures explicitly

            var tags = apiCat.Breeds?
                .SelectMany(breed => (breed.Temperament ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(tagName =>
                {
                    if (!existingTagsDict.TryGetValue(tagName, out var tag))
                    {
                        tag = new TagEntity { Name = tagName };
                        existingTagsDict[tagName] = tag;
                        db.Tags.Add(tag);
                    }
                    return tag;
                })
                .ToList() ?? [];

            newCats.Add(new CatEntity
            {
                CatId = apiCat.Id,
                Width = apiCat.Width,
                Height = apiCat.Height,
                Image = catImageResult.Data,
                Tags = tags
            });
        }

        if (newCats.Count == 0)
            return Result<FetchCatsResult>.Fail("No new unique cats fetched.");

        await db.Cats.AddRangeAsync(newCats);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return Result<FetchCatsResult>.Fail("Failed saving cats to the database.", ex);
        }

        var response = new FetchCatsResult
        {
            NewCatsCount = newCats.Count,
            DuplicateCatsCount = duplicateCount,
            Cats = newCats
        };

        return Result<FetchCatsResult>.Ok(response);
    }
    
    public async Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = db.Cats.Include(c => c.Tags).AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            query = query.Where(c => c.Tags.Any(t => t.Name == request.Tag));
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var response = new CatPaginatedResult();
        
        response.TotalItems = await query.CountAsync(cancellationToken);
        response.Cats = await query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return Result<CatPaginatedResult>.Ok(response);
    }

    public async Task<Result<CatEntity?>> GetCatByIdAsync(int id, CancellationToken cancellationToken)
    {
        CatEntity? cat = await db.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return Result<CatEntity?>.Ok(cat);
    }
    
    private async Task<Result<List<ExternalCatApiResponse>>> GetCatsAsync(int limit)
    {
        var endpoint = $"images/search?limit={limit}&has_breeds=1&api_key=live_msWm636ugdrc8vgMvwksZvkuthWIsfL29ROKuQ1GGxKNNtksoi8HUdcfbo6r5QzC";
        var result = await apiClient.GetAsync<List<ExternalCatApiResponse>>(endpoint);

        return result.Success
            ? Result<List<ExternalCatApiResponse>>.Ok(result.Data ?? new List<ExternalCatApiResponse>())
            : Result<List<ExternalCatApiResponse>>.Fail(result.ErrorMessage ?? "Failed to retrieve cats.", result.Exception);
    }

    private async Task<Result<byte[]>> GetCatImageAsync(string url)
    {
        return await apiClient.GetByteArrayAsync(url);
    }
}