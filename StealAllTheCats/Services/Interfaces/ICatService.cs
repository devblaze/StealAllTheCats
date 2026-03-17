using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Requests;
using StealAllTheCats.Dtos.Results;

namespace StealAllTheCats.Services.Interfaces;

public interface ICatService
{
    Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken ct);
    Task<Result<CatDto?>> GetCatByIdAsync(int id, CancellationToken ct);
    Task<byte[]?> GetCatImageAsync(int id, CancellationToken ct);
}
