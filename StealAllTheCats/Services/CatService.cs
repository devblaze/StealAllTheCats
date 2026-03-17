using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services.Interfaces;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class CatService(IApiClient apiClient, ApplicationDbContext db) : ICatService
{
    public async Task<List<CatEntity>> FetchCatsAsync(int limit = 25)
    {
        var apiResults = await GetCatsAsync(limit);
        var cats = new List<CatEntity>();
        
        var existingTagsDict = await db.Tags.ToDictionaryAsync(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var result in apiResults)
        {
            var imageData = await GetCatImageAsync(result.Url);

            var tags = result.Breeds?
                .SelectMany(b => (b.Temperament ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim())
                .Distinct()
                .Select(tagName =>
                {
                    if (!existingTagsDict.TryGetValue(tagName, out var tag))
                    {
                        tag = new TagEntity { Name = tagName };
                        existingTagsDict[tagName] =
                            tag;
                        db.Tags.Add(tag);
                    }
                    return tag;
                })
                .ToList() ?? [];

            var cat = new CatEntity
            {
                CatId = result.Id,
                Width = result.Width,
                Height = result.Height,
                Image = imageData,
                Tags = tags
            };

            cats.Add(cat);
        }

        await db.Cats.AddRangeAsync(cats);
        await db.SaveChangesAsync();

        return cats;
    }
    
    public async Task<CatPaginatedResponse> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = db.Cats.Include(c => c.Tags).AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            query = query.Where(c => c.Tags.Any(t => t.Name == request.Tag));
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        
        var response = new CatPaginatedResponse();
        
        response.TotalItems = await query.CountAsync(cancellationToken);
        response.Data = await query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return response;
    }

    public async Task<CatEntity?> GetCatByIdAsync(GetCatsRequest request, CancellationToken cancellationToken)
    {
        CatEntity? cat = await db.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        return cat;
    }
    
    private async Task<List<CatApiResponse>> GetCatsAsync(int limit)
    {
        var endpoint = $"images/search?limit={limit}&has_breeds=1&api_key=YOUR_API_KEY_HERE";
        var result = await apiClient.GetAsync<List<CatApiResponse>>(endpoint);
        return result ?? new List<CatApiResponse>();
    }

    private async Task<byte[]> GetCatImageAsync(string url)
    {
        return await apiClient.GetByteArrayAsync(url);
    }
}