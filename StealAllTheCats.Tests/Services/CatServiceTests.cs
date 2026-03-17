using Microsoft.EntityFrameworkCore;
using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services;
using Xunit;

namespace StealAllTheCats.Tests.Services;

public class CatServiceTests
{
    [Fact]
    public async Task FetchCatsAsync_ShouldReturnCorrectNumberOfCats()
    {
        var mockApiClient = new Mock<ICatApiClient>();

        mockApiClient.Setup(x => x.GetCatsAsync(It.IsAny<int>()))
            .ReturnsAsync([
                new CatApiResponse
                {
                    Id = "abc123",
                    Url = "https://caturl.com/img1.jpg",
                    Width = 500,
                    Height = 500,
                    Breeds = [new CatBreed { Temperament = "Playful, Curious" }]
                },

                new CatApiResponse
                {
                    Id = "def456",
                    Url = "https://caturl.com/img2.jpg",
                    Width = 600,
                    Height = 400,
                    Breeds = [new CatBreed { Temperament = "Friendly, Loyal" }]
                }
            ]);

        mockApiClient.Setup(x => x.GetCatImageAsync(It.IsAny<string>()))
            .ReturnsAsync([42]);

        var context = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        var service = new CatService(mockApiClient.Object, context.Object);

        var results = await service.FetchCatsAsync(2);

        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Collection(results,
            first => Assert.Equal("abc123", first.CatId),
            second => Assert.Equal("def456", second.CatId));

        mockApiClient.Verify(x => x.GetCatsAsync(2), Times.Once);
        mockApiClient.Verify(x => x.GetCatImageAsync(It.IsAny<string>()), Times.Exactly(2));
    }
}