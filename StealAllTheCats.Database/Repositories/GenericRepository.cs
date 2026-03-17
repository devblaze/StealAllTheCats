using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Database.Repositories.Interfaces;
using System.Linq.Expressions;

namespace StealAllTheCats.Database.Repositories;

public class GenericRepository<TEntity>(ApplicationDbContext context) : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<TEntity> Entities = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await Entities.FindAsync(id);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        return await Entities.ToListAsync();
    }

    public async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPaginatedAsync(
        int skip,
        int take,
        Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = Entities;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        int totalCount = await query.CountAsync();
        List<TEntity> items = await query.Skip(skip).Take(take).ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Entities.Where(predicate).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Entities.AnyAsync(predicate);
    }

    public async Task AddAsync(TEntity entity)
    {
        await Entities.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await Entities.AddRangeAsync(entities);
    }
    
    public Task SaveChangesAsync() => Context.SaveChangesAsync();

    public void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        Entities.RemoveRange(entities);
    }
}