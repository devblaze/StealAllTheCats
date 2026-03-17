using Microsoft.Extensions.Configuration;
using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;

public class CatServiceFixture
{
    public Mock<IGenericRepository<CatEntity>> CatRepoMock { get; } = new();
    public CatService CatService { get; }

    public CatServiceFixture()
    {
        CatService = new CatService(CatRepoMock.Object);
    }
}
