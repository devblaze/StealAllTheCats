using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;

namespace StealAllTheCats.Services;

public interface ICatService
{
    Task<List<CatEntity>> FetchCatsAsync(int limit = 25);

    Task<CatPaginatedResponse> GetCatsPaginatedAsync(GetCatsPaginatedRequest request, CancellationToken cancellationToken);
}