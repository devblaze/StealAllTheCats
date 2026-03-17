using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;

namespace StealAllTheCats.Services.Interfaces;

public interface ICatService
{
    Task<Result<FetchCatsResult>> FetchCatsAsync(int limit = 25);

    Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken cancellationToken);

    Task<Result<CatEntity?>> GetCatByIdAsync(int id, CancellationToken cancellationToken);
}