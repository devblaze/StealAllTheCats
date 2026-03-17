using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests;

public class FetchCatsTests : IClassFixture<CatServiceFixture>
{
    private readonly CatServiceFixture _fixture;

    public FetchCatsTests(CatServiceFixture fixture)
    {
        _fixture = fixture;
        fixture.ApiClientMock.Reset();
        fixture.CatRepoMock.Reset();
        fixture.TagRepoMock.Reset();
    }

    [Fact]
    public async Task Should_Report_Duplicates_Explicitly()
    {
        // Arrange
        var apiResponse = new List<ExternalCatApiResponse> { new() { Id = "cat1" } };
        _fixture.ApiClientMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiResponse));

        _fixture.CatRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _fixture.CatService.FetchCatsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.Data!.NewCatsCount);
        Assert.Equal(1, result.Data.DuplicateCatsCount);
    }

    [Fact]
    public async Task Should_Store_Fetched_Cats_Correctly_In_Database()
    {
        // Arrange
        var apiResponse = new List<ExternalCatApiResponse>
        {
            new() { Id = "cat1", Width = 800, Height = 600, Url = "url1", Breeds = [] }
        };
        _fixture.ApiClientMock.Setup(x => x.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(apiResponse));

        _fixture.CatRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CatEntity, bool>>>())).ReturnsAsync(false);

        _fixture.TagRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<TagEntity, bool>>>()))
            .ReturnsAsync(new List<TagEntity>());

        // Act
        await _fixture.CatService.FetchCatsAsync();

        // Assert
        _fixture.CatRepoMock.Verify(r => r.AddRangeAsync(It.Is<List<CatEntity>>(cats =>
            cats.Count == 1 &&
            cats[0].CatId == "cat1" &&
            cats[0].Width == 800 &&
            cats[0].Height == 600 &&
            cats[0].ImageUrl == "url1"
        )), Times.Once);

        _fixture.CatRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}