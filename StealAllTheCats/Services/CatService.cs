using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Mappers;
using StealAllTheCats.Dtos.Requests;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Services.Interfaces;
using System.Linq.Expressions;

namespace StealAllTheCats.Services;

public class CatService(
    IGenericRepository<CatEntity> catRepository) : ICatService
{
    public async Task<Result<CatPaginatedResult>> GetCatsPaginatedAsync(GetCatsRequest request, CancellationToken ct)
    {
        Expression<Func<CatEntity, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(request.Tag))
            filter = c => c.Tags.Any(t => t.Name == request.Tag);

        int skip = (request.Page - 1) * request.PageSize;
        int take = request.PageSize;

        var (cats, totalItems) = await catRepository.GetPaginatedAsync(skip, take, filter, c => c.Tags);

        return Result<CatPaginatedResult>.Ok(new CatPaginatedResult
        {
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize,
            Cats = CatMapper.ToDtoList(cats)
        });
    }

    public async Task<Result<CatDto?>> GetCatByIdAsync(int id, CancellationToken ct)
    {
        var cat = await catRepository.GetByIdAsync(id, c => c.Tags);

        if (cat == null)
            return Result<CatDto?>.Fail("Cat not found.", 404);

        return Result<CatDto?>.Ok(CatMapper.ToDto(cat));
    }

    public async Task<byte[]?> GetCatImageAsync(int id, CancellationToken ct)
    {
        var cat = await catRepository.GetByIdAsync(id);
        return cat?.ImageData;
    }
}
