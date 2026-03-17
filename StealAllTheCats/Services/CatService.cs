using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class CatService(ICatApiClient catApiClient, ApplicationDbContext db) : ICatService
{
    public async Task<List<CatEntity>> FetchCatsAsync(int limit = 25)
    {
        var apiResults = await catApiClient.GetCatsAsync(limit);

        var cats = new List<CatEntity>();

        foreach (var result in apiResults)
        {
            var imageData = await catApiClient.GetCatImageAsync(result.Url);

            var tags = result.Breeds
                .SelectMany(b => (b.Temperament ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim())
                .Distinct()
                .Select(t => new TagEntity { Name = t })
                .ToList();

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

        return cats;
    }
    
    public async Task<CatPaginatedResponse> GetCatsPaginatedAsync(GetCatsPaginatedRequest request, CancellationToken cancellationToken)
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
}