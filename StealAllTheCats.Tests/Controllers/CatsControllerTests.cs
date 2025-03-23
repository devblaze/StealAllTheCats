using Microsoft.AspNetCore.Mvc;
using Moq;
using StealAllTheCats.Controllers;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Controllers;

public class CatsControllerTests
{
    private readonly CatsController _controller;
    private readonly Mock<ICatService> _serviceMock;

    public CatsControllerTests()
    {
        _serviceMock = new Mock<ICatService>();
        _controller = new CatsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(svc => svc.GetCatByIdAsync(It.IsAny<GetCatsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CatEntity)null);

        // Act
        var result = await _controller.GetCat(new GetCatsRequest { Id = 99 }, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetCat_ShouldReturnOk_WhenCatExists()
    {
        // Arrange
        var mockedCat = new CatEntity() { Id = 1, CatId = "cat-123", Width = 200, Height = 200 };
        _serviceMock.Setup(svc => svc.GetCatByIdAsync(It.IsAny<GetCatsRequest>(), CancellationToken.None))
            .ReturnsAsync(mockedCat);

        // Act
        var result = await _controller.GetCat(new GetCatsRequest { Id = 1 }, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var cat = Assert.IsType<CatEntity>(okResult.Value);
        Assert.Equal("cat-123", cat.CatId);
    }

    [Fact]
    public async Task FetchCats_ShouldReturnOk_WhenCalledSuccessfully()
    {
        // Arrange
        var mockedCatsList = new List<CatEntity>() {
            new CatEntity { Id = 1, CatId = "cat-123", Width = 300, Height = 200 },
            new CatEntity { Id = 2, CatId = "cat-456", Width = 300, Height = 200 }
        };
        _serviceMock.Setup(service => service.FetchCatsAsync(It.IsAny<int>()))
            .ReturnsAsync(mockedCatsList);

        // Act
        var result = await _controller.FetchCats(CancellationToken.None, 2);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var cats = Assert.IsAssignableFrom<IEnumerable<CatEntity>>(okObjectResult.Value);
        Assert.Equal(2, cats.Count());
    }
}