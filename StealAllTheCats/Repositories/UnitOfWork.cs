using StealAllTheCats.Data;

namespace StealAllTheCats.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public ICatRepository Cats { get; } = new CatRepository(context);
    public ITagRepository Tags { get; } = new TagRepository(context);

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}