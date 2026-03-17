using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;

namespace StealAllTheCats.Repositories;

public class GenericRepository<TEntity>(ApplicationDbContext context) : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<TEntity> Entities = context.Set<TEntity>();public interface IUnitOfWork : IDisposable
{
    ICatRepository Cats { get; }
    ITagRepository Tags { get; }
    Task<int> CompleteAsync();
}

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await Entities.FindAsync(id);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        return await Entities.ToListAsync();
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Entities.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(TEntity entity)
    {
        await Entities.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await Entities.AddRangeAsync(entities);
    }

    public void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        Entities.RemoveRange(entities);
    }
}