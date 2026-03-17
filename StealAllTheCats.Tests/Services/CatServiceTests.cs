using Microsoft.EntityFrameworkCore;
using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Services;

public class CatServiceTests
{
    private readonly ICatService _catService;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly ApplicationDbContext _dbContext;

    public CatServiceTests()
    {
        _apiClientMock = new Mock<IApiClient>();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new ApplicationDbContext(options);
        _catService = new CatService(_apiClientMock.Object, _dbContext);
    }

    [Fact]
    public async Task FetchCatsAsync_ShouldAddCatsToDb()
    {
        // Arrange
        _apiClientMock.Setup(x => x.GetCatsAsync(It.IsAny<int>())).ReturnsAsync(new List<CatApiResponse>
        {
            new() { Id = "api-cat-1", Width = 300, Height = 300, Url = "http://cat-image.com/cat-1.jpg", 
                Breeds = new List<CatBreed>{new CatBreed{ Temperament = "Playful, Active" }}}
        });

        _apiClientMock.Setup(x => x.GetCatImageAsync(It.IsAny<string>())).ReturnsAsync(new byte[] {1, 2, 3});

        // Act
        var result = await _catService.FetchCatsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var catInDb = await _dbContext.Cats.FirstOrDefaultAsync();
        Assert.NotNull(catInDb);
        
        Assert.Equal("api-cat-1", catInDb.CatId);
        Assert.Equal(300, catInDb.Width);
        Assert.Equal(300, catInDb.Height);
        Assert.Equal(new byte[] {1, 2, 3}, catInDb.Image);
        
        var tags = catInDb.Tags.ToList();
        Assert.Equal(2, tags.Count);
        Assert.Contains(tags, tag => tag.Name == "Playful");
        Assert.Contains(tags, tag => tag.Name == "Active");
    }
}
