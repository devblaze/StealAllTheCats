using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models;

namespace StealAllTheCats.Repositories;

public class TagRepository(ApplicationDbContext context) : GenericRepository<TagEntity>(context), ITagRepository
{
    public async Task<IReadOnlyList<TagEntity>> GetTagsByNamesAsync(IEnumerable<string> names)
    {
        return await Entities.Where(t => names.Contains(t.Name)).ToListAsync();
    }
}
