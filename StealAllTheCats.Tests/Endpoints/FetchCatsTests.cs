using Xunit;
using Moq;
using StealAllTheCats.Services;
using System.Threading;
using System.Threading.Tasks;
using StealAllTheCats.Controllers;
using Microsoft.AspNetCore.Mvc;
using StealAllTheCats.Models;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Tests.Endpoints;

public class FetchCatsTests
{
    private readonly Mock<ICatService> _mockCatService;
    private readonly CatsController _catsController;

    public FetchCatsTests()
    {
        _mockCatService = new Mock<ICatService>();
        _catsController = new CatsController(_mockCatService.Object);
    }

    [Fact]
    public async Task FetchCats_ShouldReturnOkResultAndFetchedData()
    {
        // Arrange
        _mockCatService.Setup(s => s.FetchCatsAsync(It.IsAny<int>()))
            .ReturnsAsync([new() { CatId = "cat1" }, new() { CatId = "cat2" }]);

        // Act
        var result = await _catsController.FetchCats(CancellationToken.None, 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCats = Assert.IsAssignableFrom<List<CatEntity>>(okResult.Value);

        Assert.Equal(2, returnedCats.Count);
        _mockCatService.Verify(s => s.FetchCatsAsync(2), Times.Once);
    }
}