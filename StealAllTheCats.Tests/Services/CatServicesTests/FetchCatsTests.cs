using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests;

public class GetCatImageTests : IClassFixture<CatServiceFixture>
{
    private readonly CatServiceFixture _fixture;

    public GetCatImageTests(CatServiceFixture fixture)
    {
        _fixture = fixture;
        fixture.CatRepoMock.Reset();
    }

    [Fact]
    public async Task ReturnsImageDataWhenExists()
    {
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var cat = new CatEntity { Id = 1, CatId = "abc", ImageData = imageBytes };

        _fixture.CatRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync(cat);

        var result = await _fixture.CatService.GetCatImageAsync(1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(imageBytes, result);
    }

    [Fact]
    public async Task ReturnsNullWhenCatNotFound()
    {
        _fixture.CatRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((CatEntity?)null);

        var result = await _fixture.CatService.GetCatImageAsync(999, CancellationToken.None);

        Assert.Null(result);
    }
}
