using StealAllTheCats.Models.Responses;

namespace StealAllTheCats.Services
{
    public interface ICatApiClient
    {
        public Task<List<CatApiResponse>> GetCatsAsync(int limit);

        public Task<byte[]> GetCatImageAsync(string url);
    }
}