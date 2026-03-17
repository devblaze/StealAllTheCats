using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Services;

public class CatServiceTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICatService _catService;

    public CatServiceTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("CatServiceTestsDb")
            .Options;
        _dbContext = new ApplicationDbContext(dbContextOptions);

        _catService = new CatService(_mockApiClient.Object, _dbContext);
    }

    [Fact]
    public async Task FetchCatsAsync_ShouldSaveApiCatsToDatabase()
    {
        // Arrange
        var catsApiResponse = new List<CatApiResponse>
        {
            new()
            {
                Id = "cat123", Url = "http://test.com/cat1.jpg", Width = 500, Height = 400,
                Breeds = [new CatBreed { Temperament = "Calm, Loving" }]
            },
            new()
            {
                Id = "cat456", Url = "http://test.com/cat2.jpg", Width = 600, Height = 300,
                Breeds = [new CatBreed { Temperament = "Playful, Active" }]
            }
        };

        _mockApiClient.Setup(api => api.GetCatsAsync(It.IsAny<int>()))
            .ReturnsAsync(catsApiResponse);

        _mockApiClient.Setup(api => api.GetCatImageAsync(It.IsAny<string>()))
            .ReturnsAsync([0x20, 0x21]);

        // Act
        var results = await _catService.FetchCatsAsync();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal(2, await _dbContext.Cats.CountAsync());

        var storedCats = await _dbContext.Cats.Include(c => c.Tags).ToListAsync();
        Assert.True(storedCats.All(cat => cat.Image.Length == 2));
        Assert.Contains(storedCats, cat => cat.CatId == "cat123");
        Assert.Contains(storedCats, cat => cat.CatId == "cat456");
    }
}