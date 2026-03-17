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

public class CatImportServiceTests
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly Mock<IGenericRepository<CatEntity>> _catRepoMock = new();
    private readonly Mock<IGenericRepository<TagEntity>> _tagRepoMock = new();
    private readonly Mock<IGenericRepository<ImportJobEntity>> _jobRepoMock = new();
    private readonly Mock<ILogger<CatImportService>> _loggerMock = new();
    private readonly CatImportService _service;

    public CatImportServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "CatApi:ApiKey", "test-key" },
                { "CatApi:BaseUrl", "https://test" }
            })
            .Build();

        _service = new CatImportService(
            _apiClientMock.Object,
            config,
            _catRepoMock.Object,
            _tagRepoMock.Object,
            _jobRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task StartImportAsync_CreatesNewJobInQueuedState()
    {
        var result = await _service.StartImportAsync();

        Assert.Equal("queued", result.Status);
        _jobRepoMock.Verify(r => r.AddAsync(It.IsAny<ImportJobEntity>()), Times.Once);
        _jobRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetImportStatusAsync_ReturnsNullForMissingJob()
    {
        _jobRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ImportJobEntity?)null);

        var result = await _service.GetImportStatusAsync(999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetImportStatusAsync_ReturnsDtoForExistingJob()
    {
        var job = new ImportJobEntity { Id = 1, Status = ImportJobStatus.Running, Imported = 5, Skipped = 2 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var result = await _service.GetImportStatusAsync(1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("running", result!.Status);
        Assert.Equal(5, result.Imported);
        Assert.Equal(2, result.Skipped);
    }

    [Fact]
    public async Task ProcessImportAsync_SetsStatusToFailedOnApiError()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Fail("API error"));

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(ImportJobStatus.Failed, job.Status);
        Assert.Contains("API error", job.Message!);
    }

    [Fact]
    public async Task ProcessImportAsync_SkipsDuplicateCats()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new() { Id = "dup1", Url = "https://img/dup1.jpg", Width = 100, Height = 100, Breeds = [] }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));

        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(true);

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Skipped);
        Assert.Equal(0, job.Imported);
    }

    [Fact]
    public async Task ProcessImportAsync_ImportsNewCatsWithTags()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new()
            {
                Id = "new1", Url = "https://img/new1.jpg", Width = 800, Height = 600,
                Breeds = [new CatBreed { Temperament = "Friendly, Playful" }]
            }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));
        _apiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(false);
        _tagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity>());

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Imported);
        _catRepoMock.Verify(r => r.AddAsync(It.Is<CatEntity>(c =>
            c.CatId == "new1" && c.Width == 800 && c.ImageData != null)), Times.Once);
    }

    [Fact]
    public async Task ProcessImportAsync_DoesNothingForMissingJob()
    {
        _jobRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ImportJobEntity?)null);

        await _service.ProcessImportAsync(999, CancellationToken.None);

        _apiClientMock.Verify(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessImportAsync_ReusesExistingTags()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new()
            {
                Id = "cat1", Url = "https://img/cat1.jpg", Width = 100, Height = 100,
                Breeds = [new CatBreed { Temperament = "Friendly, Lazy" }]
            }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));
        _apiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 1 });

        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(false);

        var existingTag = new TagEntity { Id = 10, Name = "Friendly" };
        _tagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity> { existingTag });

        await _service.ProcessImportAsync(1, CancellationToken.None);

        _tagRepoMock.Verify(r => r.AddRangeAsync(It.Is<List<TagEntity>>(tags =>
            tags.Count == 1 && tags[0].Name == "Lazy")), Times.Once);
    }

    [Fact]
    public async Task ProcessImportAsync_HandlesImageDownloadFailureGracefully()
    {
        var job = new ImportJobEntity { Id = 1 };
        _jobRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

        var apiCats = new List<ExternalCatApiResponse>
        {
            new() { Id = "cat1", Url = "https://img/cat1.jpg", Width = 100, Height = 100, Breeds = [] }
        };
        _apiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiCats));
        _apiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("timeout"));

        _catRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(false);
        _tagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity>());

        await _service.ProcessImportAsync(1, CancellationToken.None);

        Assert.Equal(ImportJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Imported);
        _catRepoMock.Verify(r => r.AddAsync(It.Is<CatEntity>(c => c.ImageData == null)), Times.Once);
    }
}
