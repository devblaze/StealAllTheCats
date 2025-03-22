using StealAllTheCats.Models.Responses;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class CatApiClient : ICatApiClient
{
    private readonly HttpClient _http;

    public CatApiClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.BaseAddress = new Uri("https://api.thecatapi.com/v1/");
    }

    public async Task<List<CatApiResponse>> GetCatsAsync(int limit)
    {
        var response = await _http.GetStringAsync($"images/search?limit={limit}&has_breeds=1");
        return JsonSerializer.Deserialize<List<CatApiResponse>>(response) ?? [];
    }

    public Task<byte[]> GetCatImageAsync(string url)
    {
        return _http.GetByteArrayAsync(url);
    }
}