using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos.Requets;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests;

public class GetCatsPaginatedAsyncTests : IClassFixture<CatServiceFixture>
{
    private readonly CatServiceFixture _fixture;

    public GetCatsPaginatedAsyncTests(CatServiceFixture fixture)
    {
        _fixture = fixture;
        fixture.CatRepoMock.Reset();
    }

    [Fact]
    public async Task Should_Return_Correct_Pagination_And_Tag_Filtering()
    {
        // Arrange
        var request = new GetCatsRequest { Page = 1, PageSize = 10, Tag = "Friendly" };

        var cats = new List<CatEntity>
        {
            new() { Id = 1, CatId = "cat1", Tags = [new TagEntity { Name = "Friendly" }] }
        };

        _fixture.CatRepoMock.Setup(r => r.GetPaginatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CatEntity, bool>>?>()))
            .ReturnsAsync((cats, 1));

        // Act
        var result = await _fixture.CatService.GetCatsPaginatedAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data!.Cats);
        Assert.Equal("cat1", result.Data.Cats.First().CatId);
        Assert.Equal(1, result.Data.TotalItems);
    }
}