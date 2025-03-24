using StealAllTheCats.Models;

namespace StealAllTheCats.Repositories;

public interface ICatRepository : IGenericRepository<CatEntity>
{
    Task<bool> CatExistsAsync(string catId);
    Task<IReadOnlyList<CatEntity>> GetCatsPaginatedAsync(int skip, int take, string? tag);
}
