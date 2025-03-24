using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models;

namespace StealAllTheCats.Repositories;

public class CatRepository(ApplicationDbContext context) : GenericRepository<CatEntity>(context), ICatRepository
{
    public Task<bool> CatExistsAsync(string catId)
    {
        return Entities.AnyAsync(c => c.CatId == catId);
    }

    public async Task<IReadOnlyList<CatEntity>> GetCatsPaginatedAsync(int skip, int take, string? tag)
    {
        IQueryable<CatEntity> query = Entities.Include(c => c.Tags);

        if (!string.IsNullOrEmpty(tag))
            query = query.Where(c => c.Tags!.Any(t => t.Name == tag));

        return await query.Skip(skip).Take(take).ToListAsync();
    }
}