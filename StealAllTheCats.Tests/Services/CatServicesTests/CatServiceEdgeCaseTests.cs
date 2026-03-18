using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos.Requests;
using StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests;

public class CatServiceEdgeCaseTests : IClassFixture<CatServiceFixture>
{
    private readonly CatServiceFixture _fixture;

    public CatServiceEdgeCaseTests(CatServiceFixture fixture)
    {
        _fixture = fixture;
        fixture.CatRepoMock.Reset();
    }

    [Fact]
    public async Task GetCatsPaginated_WithNoTag_PassesNullFilter()
    {
        var request = new GetCatsRequest { Page = 1, PageSize = 10 };

        _fixture.CatRepoMock.Setup(r => r.GetPaginatedAsync(
                0, 10, null, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((new List<CatEntity>(), 0));

        var result = await _fixture.CatService.GetCatsPaginatedAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.Cats);
        Assert.Equal(0, result.Data.TotalItems);
    }

    [Fact]
    public async Task GetCatsPaginated_WithEmptyTag_PassesNullFilter()
    {
        var request = new GetCatsRequest { Page = 1, PageSize = 10, Tag = "   " };

        _fixture.CatRepoMock.Setup(r => r.GetPaginatedAsync(
                0, 10, null, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((new List<CatEntity>(), 0));

        var result = await _fixture.CatService.GetCatsPaginatedAsync(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetCatById_ReturnsCatWithEmptyTags()
    {
        var cat = new CatEntity { Id = 1, CatId = "abc", Tags = new List<TagEntity>() };

        _fixture.CatRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync(cat);

        var result = await _fixture.CatService.GetCatByIdAsync(1, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.Tags);
    }

    [Fact]
    public async Task GetCatImage_ReturnsNullForCatWithNoImage()
    {
        var cat = new CatEntity { Id = 1, CatId = "abc", ImageData = null };

        _fixture.CatRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync(cat);

        var result = await _fixture.CatService.GetCatImageAsync(1, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCatsPaginated_IncludesPageInfoInResult()
    {
        var request = new GetCatsRequest { Page = 2, PageSize = 5 };

        _fixture.CatRepoMock.Setup(r => r.GetPaginatedAsync(
                5, 5, null, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((new List<CatEntity>(), 15));

        var result = await _fixture.CatService.GetCatsPaginatedAsync(request, CancellationToken.None);

        Assert.Equal(2, result.Data!.Page);
        Assert.Equal(5, result.Data.PageSize);
        Assert.Equal(15, result.Data.TotalItems);
    }
}
