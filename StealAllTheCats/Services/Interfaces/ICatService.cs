using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Requets;
using StealAllTheCats.Dtos.Results;

namespace StealAllTheCats.Services.Interfaces;

public interface ICatService
{
    Task<Result<FetchCatsResult>> FetchCatsAsync(int limit = 25);

    Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken);

    Task<Result<CatDto?>> GetCatByIdAsync(int id, CancellationToken cancellationToken);
}