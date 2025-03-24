using StealAllTheCats.Models;

namespace StealAllTheCats.Repositories;

public interface ITagRepository : IGenericRepository<TagEntity>
{
    Task<IReadOnlyList<TagEntity>> GetTagsByNamesAsync(IEnumerable<string> names);
}
