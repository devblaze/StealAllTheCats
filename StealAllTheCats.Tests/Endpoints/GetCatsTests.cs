using Microsoft.AspNetCore.Mvc;
using Moq;
using StealAllTheCats.Controllers;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Endpoints;

public class GetCatsControllerTests
{
    private readonly Mock<ICatService> _mockCatService;
    private readonly CatsController _catsController;

    public GetCatsControllerTests()
    {
        _mockCatService = new Mock<ICatService>();
        _catsController = new CatsController(_mockCatService.Object);
    }

    [Fact]
    public async Task GetCats_Returns_Ok_With_Correct_Data()
    {
        // Arrange
        var request = new GetCatsRequest
        {
            Tag = "Playful",
            Page = 1,
            PageSize = 10
        };

        var paginatedResponse = new CatPaginatedResponse
        {
            TotalItems = 2,
            Data = new List<CatEntity>
            {
                new() { Id = 1, CatId = "cat1", Tags = [new TagEntity { Name = "Playful" }] },
                new() { Id = 2, CatId = "cat2", Tags = [new TagEntity { Name = "Playful" }] }
            }
        };

        _mockCatService
            .Setup(service => service.GetCatsPaginatedAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        // Act
        var result = await _catsController.GetCats(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseData = Assert.IsType<CatPaginatedResponse>(okResult.Value);

        Assert.Equal(2, responseData.TotalItems);
        Assert.All(responseData.Data,
            cat =>
                Assert.Contains(cat.Tags, t => t.Name == "Playful"));
        
        _mockCatService.Verify(
            service => service.GetCatsPaginatedAsync(request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCat_With_Valid_Id_Should_Return_OkResult()
    {
        // Arrange
        var request = new GetCatsRequest { Id = 1 };

        var catEntity = new CatEntity { Id = 1, CatId = "cat1" };

        _mockCatService
            .Setup(service => service.GetCatByIdAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(catEntity);

        // Act
        var result = await _catsController.GetCat(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCat = Assert.IsType<CatEntity>(okResult.Value);
        Assert.Equal(catEntity.Id, returnedCat.Id);
        Assert.Equal(catEntity.CatId, returnedCat.CatId);
        _mockCatService.Verify(s => s.GetCatByIdAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCat_With_Invalid_Id_Should_Return_NotFound()
    {
        // Arrange
        var request = new GetCatsRequest { Id = 1000 };

        _mockCatService
            .Setup(service => service.GetCatByIdAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CatEntity?)null);

        // Act
        var result = await _catsController.GetCat(request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockCatService.Verify(s => s.GetCatByIdAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}