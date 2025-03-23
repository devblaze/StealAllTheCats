using Microsoft.EntityFrameworkCore;
using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
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
    public async Task FetchCatsAsync_Should_Report_Duplicates_Explicitly()
    {
        // Arrange
        _apiClientMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>
            {
                new()
                {
                    Id = "api-cat-1", Width = 300, Height = 300, Url = "http://cat-image.com/cat-1.jpg",
                    Breeds = new List<CatBreed> { new() { Temperament = "Playful, Active" } }
                },
                new()
                {
                    Id = "api-cat-2", Width = 400, Height = 400, Url = "http://cat-image.com/cat-2.jpg",
                    Breeds = new List<CatBreed> { new() { Temperament = "Calm, Sleepy" } }
                }
            }));

        _apiClientMock.Setup(x => x.GetByteArrayAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Ok(new byte[] { 1, 2, 3 }));

        // Act - First run to fetch unique cats into DB
        var firstRunResult = await _catService.FetchCatsAsync();

        // Explicitly verify no duplicates in first call
        Assert.True(firstRunResult.Success);
        Assert.Equal(2, firstRunResult.Data!.NewCatsCount);
        Assert.Equal(0, firstRunResult.Data.DuplicateCatsCount);

        // Act - Second fetch attempt, expecting duplicates clearly
        var secondRunResult = await _catService.FetchCatsAsync();

        // Explicit assert for second call: expecting 2 duplicates
        Assert.False(secondRunResult.Success); // Should fail because we have no new cats explicitly
        Assert.Equal("No new unique cats fetched.", secondRunResult.ErrorMessage);

        // Now simulate we return 1 existing cat + 1 totally new cat
        _apiClientMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>
            {
                new()
                {
                    Id = "api-cat-1", Width = 300, Height = 300, Url = "http://cat-image.com/cat-1.jpg",
                    Breeds = new List<CatBreed> { new() { Temperament = "Playful, Active" } }
                }, // existing cat explicitly
                new()
                {
                    Id = "api-cat-3", Width = 500, Height = 500, Url = "http://cat-image.com/cat-3.jpg",
                    Breeds = new List<CatBreed> { new() { Temperament = "Friendly" } }
                } // new cat
            }));

        var thirdRunResult = await _catService.FetchCatsAsync();

        Assert.True(thirdRunResult.Success);
        Assert.Equal(1, thirdRunResult.Data!.NewCatsCount); // 1 new cat explicitly
        Assert.Equal(1, thirdRunResult.Data.DuplicateCatsCount); // explicitly expecting 1 duplicate

        // Confirm Cats table state explicitly
        Assert.Equal(3, await _dbContext.Cats.CountAsync());

        // Confirm Tags database explicitly
        var tagsInDb = await _dbContext.Tags.Select(t => t.Name).ToListAsync();
        Assert.Equal(5, tagsInDb.Count); // "Playful","Active","Calm","Sleepy","Friendly"
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

        _apiClientMock.Setup(x => x.GetByteArrayAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Ok(new byte[] { 10, 20, 30 }));

        // Act
        var result = await _catService.FetchCatsAsync(2);

        // Assert
        Assert.True(result.Success);
        var storedCats = await _dbContext.Cats.ToListAsync();
        Assert.Equal(2, storedCats.Count);

        Assert.Contains(storedCats, c => c.CatId == "cat-001" && c.Image.SequenceEqual(new byte[] { 10, 20, 30 }));
        Assert.Contains(storedCats, c => c.CatId == "cat-002" && c.Image.SequenceEqual(new byte[] { 10, 20, 30 }));
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

        _apiClientMock.Setup(x => x.GetByteArrayAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Ok(new byte[] { 5, 5, 5 }));

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
                Image = new byte[] { 5, 10, 15 },
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
