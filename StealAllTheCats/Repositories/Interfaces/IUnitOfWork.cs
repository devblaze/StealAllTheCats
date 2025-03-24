namespace StealAllTheCats.Repositories;

public interface IUnitOfWork : IDisposable
{
    ICatRepository Cats { get; }
    ITagRepository Tags { get; }
    Task<int> CompleteAsync();
}