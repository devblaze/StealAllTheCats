using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Requets;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Models;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Services;

public class CatServiceTests
{
    private readonly ICatService _catService;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ApplicationDbContext _dbContext;

    public CatServiceTests()
    {
        _apiClientMock = new Mock<IApiClient>();
        _configurationMock = new Mock<IConfiguration>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _catService = new CatService(_dbContext, _apiClientMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task FetchCatsAsync_Should_Report_Duplicates_Explicitly()
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApplicationDbContext(dbOptions);
        
        context.Cats.Add(new CatEntity { CatId = "cat-123", Width = 200, Height = 100, ImageUrl = "ttp://example.com/cat123.png" });
        await context.SaveChangesAsync();
        
        var apiMock = new Mock<IApiClient>();
        apiMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>
            {
                new() { Id = "cat-123", Url = "http://example.com/cat123.png", Width = 200, Height = 100 }
            }));

        var configurationMock = new Mock<IConfiguration>();
        var catService = new CatService(context, apiMock.Object, configurationMock.Object);

        // Act
        var result = await catService.FetchCatsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        Assert.Equal(0, result.Data!.NewCatsCount);
        Assert.Equal(1, result.Data.DuplicateCatsCount);
        Assert.Empty(result.Data.Cats);
    }

    [Fact]
    public async Task FetchCatsAsync_ShouldStoreFetchedCatsCorrectlyInDatabase()
    {
        // Arrange
        _apiClientMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>
            {
                new() { Id = "cat-001", Width = 300, Height = 300, Url = "http://cat-image.com/cat-001.jpg" },
                new() { Id = "cat-002", Width = 400, Height = 200, Url = "http://cat-image.com/cat-002.jpg" },
            }));

        // Act
        var result = await _catService.FetchCatsAsync(2);

        // Assert
        Assert.True(result.Success);
        var storedCats = await _dbContext.Cats.ToListAsync();
        Assert.Equal(2, storedCats.Count);

        Assert.Contains(storedCats, c => c.CatId == "cat-001" && c.ImageUrl == "http://cat-image.com/cat-001.jpg");
        Assert.Contains(storedCats, c => c.CatId == "cat-002" && c.ImageUrl == "http://cat-image.com/cat-002.jpg");
    }

    [Fact]
    public async Task FetchCatsAsync_ShouldStoreAndRelateTagsCorrectly()
    {
        // Arrange
        _apiClientMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>
            {
                new()
                {
                    Id = "cat-101",
                    Width = 350,
                    Height = 350,
                    Url = "http://cats.com/cat-101.jpg",
                    Breeds = new List<CatBreed> { new() { Temperament = "Friendly,Lazy" } }
                }
            }));

        // Act
        var result = await _catService.FetchCatsAsync(1);

        // Assert
        Assert.True(result.Success);

        var catInDb = await _dbContext.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.CatId == "cat-101");
        Assert.NotNull(catInDb);
        Assert.Equal(2, catInDb.Tags.Count);
        Assert.Contains(catInDb.Tags, t => t.Name == "Friendly");
        Assert.Contains(catInDb.Tags, t => t.Name == "Lazy");
    }

    [Fact]
    public async Task GetCatsPaginatedAsync_ShouldReturnCorrectPaginationAndTagFiltering()
    {
        // Arrange
        var tags = new[]
        {
            new TagEntity { Name = "Playful" },
            new TagEntity { Name = "Sleepy" }
        };
        await _dbContext.Tags.AddRangeAsync(tags);
        await _dbContext.SaveChangesAsync();

        var cats = new List<CatEntity>();
        for (int i = 1; i <= 15; i++)
        {
            cats.Add(new CatEntity
            {
                CatId = $"cat-{i}",
                Width = 300,
                Height = 300,
                ImageUrl = "http://cats.com/cat-101.jpg",
                Tags = (i % 2 == 0) ? [tags[0]] : [tags[1]]
            });
        }

        await _dbContext.Cats.AddRangeAsync(cats);
        await _dbContext.SaveChangesAsync();

        // Act
        var request = new GetCatsRequest
        {
            Page = 1,
            PageSize = 5,
            Tag = "Playful"
        };

        var paginatedResult = await _catService.GetCatsPaginatedAsync(request, CancellationToken.None);

        // Assert
        Assert.True(paginatedResult.Success);
        Assert.NotNull(paginatedResult.Data);
        Assert.Equal(5, paginatedResult.Data.Cats.Count);
        Assert.Equal(7, paginatedResult.Data.TotalItems); // Total 7 matching the "Playful" tag (even-numbered)

        foreach (var catDto in paginatedResult.Data.Cats)
        {
            var dbCat = await _dbContext.Cats.Include(c => c.Tags).FirstAsync(c => c.CatId == catDto.CatId);
            Assert.Contains(dbCat.Tags, t => t.Name == "Playful");
        }
    }
}
