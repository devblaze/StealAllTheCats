using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatImportServiceTests;

public class CatImportEdgeCaseTests
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly Mock<IGenericRepository<CatEntity>> _catRepoMock = new();
    private readonly Mock<IGenericRepository<TagEntity>> _tagRepoMock = new();
    private readonly Mock<IGenericRepository<ImportJobEntity>> _jobRepoMock = new();
    private readonly CatImportService _service;

    public CatImportEdgeCaseTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CatApi:ApiKey", "test-key" },
                { "CatApi:BaseUrl", "https://test" }
            })
            .Build();

        _service = new CatImportService(
            _apiClientMock.Object, config,
            _catRepoMock.Object, _tagRepoMock.Object,
            _jobRepoMock.Object, new Mock<ILogger<CatImportService>>().Object);
    }

    [Fact]
    public async Task ProcessImport_HandlesEmptyApiResponse()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>()));

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(0, job.Imported);
        Assert.Equal(0, job.Skipped);
    }

    [Fact]
    public async Task ProcessImport_HandlesNullTemperament()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new()
            {
                Id = "cat1", Url = "https://img/cat1.jpg", Width = 100, Height = 100,
                Breeds = [new CatBreed { Temperament = null }]
            }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));
        _apiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 1 });
        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(false);
        _tagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity>());

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(1, job.Imported);
        // No tags should be created since temperament was null
        _tagRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<List<TagEntity>>()), Times.Never);
    }

    [Fact]
    public async Task ProcessImport_TrimsTemperamentWhitespace()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new()
            {
                Id = "cat1", Url = "https://img/cat1.jpg", Width = 100, Height = 100,
                Breeds = [new CatBreed { Temperament = "  Playful  ,  Friendly  " }]
            }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));
        _apiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 1 });
        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(false);
        _tagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity>());

        await _service.ProcessImportAsync(1, CancellationToken.None);

        _tagRepoMock.Verify(r => r.AddRangeAsync(It.Is<List<TagEntity>>(tags =>
            tags.All(t => !t.Name.StartsWith(" ") && !t.Name.EndsWith(" ")))), Times.Once);
    }

    [Fact]
    public async Task ProcessImport_SetsCompletedTimestamp()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>()));

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.NotNull(job.Completed);
    }

    [Fact]
    public async Task ProcessImport_CountsPerCatFailures()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new() { Id = "cat1", Url = "https://img/1.jpg", Width = 100, Height = 100, Breeds = [] },
            new() { Id = "cat2", Url = "https://img/2.jpg", Width = 100, Height = 100, Breeds = [] }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));
        _apiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 1 });

        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(false);
        _tagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity>());

        // First cat saves fine, second throws
        var callCount = 0;
        _catRepoMock.Setup(r => r.AddAsync(It.IsAny<CatEntity>()))
            .Returns<CatEntity>(c =>
            {
                callCount++;
                if (callCount == 2)
                    throw new Exception("DB write failed");
                return Task.CompletedTask;
            });

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Imported);
        Assert.Equal(1, job.Failed);
    }
}
