using Microsoft.AspNetCore.Mvc;
using Moq;
using StealAllTheCats.Controllers;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Requests;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Controllers;

public class CatsControllerTests
{
    private readonly Mock<ICatService> _catServiceMock = new();
    private readonly CatsController _controller;

    public CatsControllerTests()
    {
        _controller = new CatsController(_catServiceMock.Object);
    }

    [Fact]
    public async Task GetCats_ReturnsOkWithPaginatedResult()
    {
        var expected = new CatPaginatedResult { TotalItems = 2, Page = 1, PageSize = 10, Cats = [new CatDto { Id = 1 }] };
        _catServiceMock.Setup(s => s.GetCatsPaginatedAsync(It.IsAny<GetCatsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CatPaginatedResult>.Ok(expected));

        var result = await _controller.GetCats(new GetCatsRequest(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task GetCat_ReturnsOkWhenFound()
    {
        var catDto = new CatDto { Id = 1, CatId = "abc" };
        _catServiceMock.Setup(s => s.GetCatByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CatDto?>.Ok(catDto));

        var result = await _controller.GetCat(1, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCat_Returns404WhenNotFound()
    {
        _catServiceMock.Setup(s => s.GetCatByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CatDto?>.Fail("Not found", 404));

        var result = await _controller.GetCat(999, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetCatImage_ReturnsFileWhenImageExists()
    {
        var imageBytes = new byte[] { 0xFF, 0xD8 };
        _catServiceMock.Setup(s => s.GetCatImageAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageBytes);

        var result = await _controller.GetCatImage(1, CancellationToken.None);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/jpeg", fileResult.ContentType);
        Assert.Equal(imageBytes, fileResult.FileContents);
    }

    [Fact]
    public async Task GetCatImage_ReturnsNotFoundWhenNoImage()
    {
        _catServiceMock.Setup(s => s.GetCatImageAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _controller.GetCatImage(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
