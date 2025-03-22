using StealAllTheCats.Models;

namespace StealAllTheCats.Services;

public interface ICatService
{
    Task<List<CatEntity>> FetchCatsAsync(int limit = 25);
}