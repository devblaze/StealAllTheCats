using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests;

public class GetCatByIdTests : IClassFixture<CatServiceFixture>
{
    private readonly CatServiceFixture _fixture;

    public GetCatByIdTests(CatServiceFixture fixture)
    {
        _fixture = fixture;
        fixture.CatRepoMock.Reset();
    }

    [Fact]
    public async Task ReturnsTagsAndImageUrl()
    {
        var cat = new CatEntity
        {
            Id = 50,
            CatId = "hBXicehMA",
            ImageUrl = "https://example.com/cat50.jpg",
            Tags = new List<TagEntity> { new() { Name = "Playful" }, new() { Name = "Cuddly" } }
        };

        _fixture.CatRepoMock
            .Setup(r => r.GetByIdAsync(50, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync(cat);

        var result = await _fixture.CatService.GetCatByIdAsync(50, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("https://example.com/cat50.jpg", result.Data!.ImageUrl);
        Assert.Contains("Playful", result.Data.Tags);
        Assert.Contains("Cuddly", result.Data.Tags);
    }

    [Fact]
    public async Task ReturnsFailWhenCatNotFound()
    {
        _fixture.CatRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((CatEntity?)null);

        var result = await _fixture.CatService.GetCatByIdAsync(999, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.ErrorCode);
    }
}
