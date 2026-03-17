using Microsoft.AspNetCore.Mvc;
using Moq;
using StealAllTheCats.BackgroundJobs;
using StealAllTheCats.Controllers;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Services.Interfaces;
using Xunit;

namespace StealAllTheCats.Tests.Controllers;

public class CatImportsControllerTests
{
    private readonly Mock<ICatImportService> _importServiceMock = new();
    private readonly ImportQueue _queue = new();
    private readonly CatImportsController _controller;

    public CatImportsControllerTests()
    {
        _controller = new CatImportsController(_importServiceMock.Object, _queue);
    }

    [Fact]
    public async Task StartImport_ReturnsAcceptedWithJobId()
    {
        var jobDto = new ImportJobDto { Id = 42, Status = "queued" };
        _importServiceMock.Setup(s => s.StartImportAsync()).ReturnsAsync(jobDto);

        var result = await _controller.StartImport();

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task GetImportStatus_ReturnsOkWhenJobExists()
    {
        var jobDto = new ImportJobDto { Id = 1, Status = "completed", Imported = 10 };
        _importServiceMock.Setup(s => s.GetImportStatusAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobDto);

        var result = await _controller.GetImportStatus(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(jobDto, ok.Value);
    }

    [Fact]
    public async Task GetImportStatus_ReturnsNotFoundForMissingJob()
    {
        _importServiceMock.Setup(s => s.GetImportStatusAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportJobDto?)null);

        var result = await _controller.GetImportStatus(999, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
