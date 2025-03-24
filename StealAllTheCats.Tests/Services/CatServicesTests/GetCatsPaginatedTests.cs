using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos.Requets;
using StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests;

public class GetCatsPaginatedTests : IClassFixture<CatServiceFixture>
{
    private readonly CatServiceFixture _fixture;

    public GetCatsPaginatedTests(CatServiceFixture fixture)
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
        
        _fixture.CatRepoMock.Setup(r => r.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<CatEntity, bool>>>(),
                It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((cats, 1));

        // Act
        var result = await _fixture.CatService.GetCatsPaginatedAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data!.Cats);
        Assert.Equal("cat1", result.Data.Cats.First().CatId);
        Assert.Equal(1, result.Data.TotalItems);
    }
    
    [Fact]
    public async Task GetCatsPaginated_ShouldReturnTagsAndImageUrl()
    {
        // Arrange
        var request = new GetCatsRequest { Page = 1, PageSize = 10 };

        var cats = new List<CatEntity>
        {
            new CatEntity
            {
                Id = 1,
                CatId = "cat1",
                ImageUrl = "https://example.com/cat.jpg",
                Tags = new List<TagEntity> { new() { Name = "Friendly" }, new() { Name = "Lazy" } }
            }
        };

        _fixture.CatRepoMock.Setup(r => r.GetPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                null,
                It.IsAny<Expression<Func<CatEntity, object>>[]>()))
            .ReturnsAsync((cats, 1));

        // Act
        var result = await _fixture.CatService.GetCatsPaginatedAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data.Cats);
        var catDto = result.Data.Cats.First();
        Assert.Equal("https://example.com/cat.jpg", catDto.ImageUrl);
        Assert.Contains("Friendly", catDto.Tags);
        Assert.Contains("Lazy", catDto.Tags);
    }
}