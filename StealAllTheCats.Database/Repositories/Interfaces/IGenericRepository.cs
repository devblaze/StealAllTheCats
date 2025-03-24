using System.Linq.Expressions;

namespace StealAllTheCats.Database.Repositories.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<TEntity>> GetAllAsync();

    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPaginatedAsync(int skip, int take, Expression<Func<TEntity, bool>>? filter = null);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    Task SaveChangesAsync();
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}