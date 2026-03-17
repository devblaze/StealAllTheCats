using StealAllTheCats.Services.Interfaces;
using System.Text.Json;

namespace StealAllTheCats.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _http = httpClient;
        _http.BaseAddress = new Uri("https://api.thecatapi.com/v1/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            var response = await _http.GetStringAsync(url);
            return JsonSerializer.Deserialize<T>(response, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public Task<byte[]> GetByteArrayAsync(string url)
    {
        return _http.GetByteArrayAsync(url);
    }
}