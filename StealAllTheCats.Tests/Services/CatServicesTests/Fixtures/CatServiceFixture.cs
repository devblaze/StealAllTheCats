using Microsoft.Extensions.Configuration;
using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;

public class CatServiceFixture
{
    public Mock<IApiClient> ApiClientMock { get; } = new();
    public Mock<IGenericRepository<CatEntity>> CatRepoMock { get; } = new();
    public Mock<IGenericRepository<TagEntity>> TagRepoMock { get; } = new();
    public IConfiguration Configuration { get; }

    public CatService CatService { get; }

    public CatServiceFixture()
    {
        var settings = new Dictionary<string, string>
        {
            {"CatApi:ApiKey", "test-key"},
            {"CatApi:BaseUrl", "https://test-api-url"}
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        CatService = new CatService(ApiClientMock.Object, Configuration, CatRepoMock.Object, TagRepoMock.Object);
    }
}