namespace StealAllTheCats.Services.Interfaces;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string url);

    Task<byte[]> GetByteArrayAsync(string url);
}