using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Models.Results;

namespace StealAllTheCats.Services.Interfaces;

public interface ICatService
{
    Task<List<CatEntity>> FetchCatsAsync(int limit = 25);

    Task<CatPaginatedResult> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken);

    Task<CatEntity?> GetCatByIdAsync(int id, CancellationToken cancellationToken);
}