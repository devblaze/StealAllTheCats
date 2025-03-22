using StealAllTheCats.Models.Responses;

namespace StealAllTheCats.Services.Interfaces
{
    public interface IApiClient
    {
        public Task<List<CatApiResponse>> GetCatsAsync(int limit);

        public Task<byte[]> GetCatImageAsync(string url);
    }
}