using StealAllTheCats.Dtos;
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

    public async Task<Result<T>> GetAsync<T>(string url)
    {
        try
        {
            var response = await _http.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<T>(response, _jsonOptions);

            if (data is null)
                return Result<T>.Fail("Deserialization returned null.");

            return Result<T>.Ok(data);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail("Error occurred during request.", ex);
        }
    }
}