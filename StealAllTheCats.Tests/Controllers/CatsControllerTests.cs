using Microsoft.AspNetCore.Mvc;
using Moq;
using StealAllTheCats.Controllers;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Models;
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
        _serviceMock.Setup(svc => svc.GetCatByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CatDto?>.Fail("Cat not found."));

        // Act
        var result = await _controller.GetCat(99, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCat_ShouldReturnOk_WhenCatExists()
    {
        // Arrange
        var mockedCatDto = new CatDto() 
        { 
            Id = 1, 
            CatId = "cat-123", 
            Width = 200, 
            Height = 200,
            ImageUrl = "http://example.com/cat123.png"
        };
        _serviceMock.Setup(svc => svc.GetCatByIdAsync(1, CancellationToken.None))
            .ReturnsAsync(Result<CatDto?>.Ok(mockedCatDto));

        // Act
        var result = await _controller.GetCat(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultData = Assert.IsType<CatDto>(okResult.Value);
        Assert.Equal("cat-123", resultData.CatId);
    }

    [Fact]
    public async Task FetchCats_ShouldReturnOk_WhenCalledSuccessfully()
    {
        // Arrange
        var mockedCatsList = new List<CatDto>() {
            new CatDto { Id = 1, CatId = "cat-123", Width = 300, Height = 200 },
            new CatDto { Id = 2, CatId = "cat-456", Width = 300, Height = 200 }
        };
        _serviceMock.Setup(service => service.FetchCatsAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<FetchCatsResult>.Ok(new FetchCatsResult
            {
                NewCatsCount = mockedCatsList.Count,
                DuplicateCatsCount = 0,
                Cats = mockedCatsList
            }));

        // Act
        var result = await _controller.FetchCats(CancellationToken.None, 2);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var resultData = Assert.IsType<FetchCatsResult>(okObjectResult.Value);
        var cats = resultData.Cats;
        Assert.Equal(2, cats.Count());
    }
}