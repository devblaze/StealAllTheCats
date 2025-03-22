using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services.Interfaces;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.BaseAddress = new Uri("https://api.thecatapi.com/v1/");
    }

    // generic
    public async Task<List<CatApiResponse>> GetCatsAsync(int limit)
    {
        string response;
        
        try
        {
            response = await _http.GetStringAsync($"images/search?limit={limit}&has_breeds=1&api_key=live_msWm636ugdrc8vgMvwksZvkuthWIsfL29ROKuQ1GGxKNNtksoi8HUdcfbo6r5QzC");
        }
        catch (Exception e)
        {
            return new(); // result pattern
        }
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
        return JsonSerializer.Deserialize<List<CatApiResponse>>(response, options) ?? [];
    }
    
    public Task<byte[]> GetCatImageAsync(string url)
    {
        return _http.GetByteArrayAsync(url);
    }
}