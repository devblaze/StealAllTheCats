using Moq;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Tests.Services.CatServicesTests.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace StealAllTheCats.Tests.Services.CatServicesTests
{
    public class GetCatByIdTests: IClassFixture<CatServiceFixture>
    {
        private readonly CatServiceFixture _fixture;

        public GetCatByIdTests(CatServiceFixture fixture)
        {
            _fixture = fixture;
            fixture.ApiClientMock.Reset();
            fixture.CatRepoMock.Reset();
            fixture.TagRepoMock.Reset();
        }
        
        [Fact]
        public async Task GetCatById_ShouldReturnTagsAndImageUrl()
        {
            // Arrange
            var cat = new CatEntity
            {
                Id = 50,
                CatId = "hBXicehMA",
                ImageUrl = "https://example.com/cat50.jpg",
                Tags = new List<TagEntity> { new() { Name = "Playful" }, new() { Name = "Cuddly" } }
            };

            _fixture.CatRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<Expression<Func<CatEntity, object>>[]>()))
                .ReturnsAsync(cat);

            // Act
            var result = await _fixture.CatService.GetCatByIdAsync(50, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            var catDto = result.Data!;
            Assert.Equal("https://example.com/cat50.jpg", catDto.ImageUrl);
            Assert.Contains("Playful", catDto.Tags);
            Assert.Contains("Cuddly", catDto.Tags);
        }

    }
}