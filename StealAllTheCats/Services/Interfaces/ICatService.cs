using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;

namespace StealAllTheCats.Services.Interfaces;

public interface ICatService
{
    Task<List<CatEntity>> FetchCatsAsync(int limit = 25);

    Task<CatPaginatedResponse> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken);

    Task<CatEntity?> GetCatByIdAsync(GetCatsRequest request, CancellationToken cancellationToken);
}