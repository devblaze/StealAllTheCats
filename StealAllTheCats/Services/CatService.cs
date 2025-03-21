using StealAllTheCats.Models;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class CatService
{
    private readonly HttpClient _client;

    public CatService(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<CatEntity>> FetchCatsAsync(int limit = 25)
    {
        var response = await _client.GetStringAsync($"images/search?limit={limit}&has_breeds=1");
        var results = JsonSerializer.Deserialize<List<CatApiResponse>>(response);

        return results?.Select(cat => new CatEntity
        {
            CatId = cat.Id,
            Width = cat.Width,
            Height = cat.Height,
            Image = Convert.FromBase64String(cat.Url),
            Tags = cat.Breeds.FirstOrDefault()?.Temperament?
                .Split(',')
                .Select(t => new TagEntity { Name = t.Trim() })
                .ToList()
        }).ToList() ?? new List<CatEntity>();
    }
    
    private class CatApiResponse
    {
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }

        public List<CatBreed> Breeds { get; set; } = new();
    }

    private class CatBreed
    {
        public string? Temperament { get; set; }
    }
}