using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos.Mappers;
using Xunit;

namespace StealAllTheCats.Tests.Mappers;

public class CatMapperTests
{
    [Fact]
    public void ToDto_MapsAllFieldsCorrectly()
    {
        var entity = new CatEntity
        {
            Id = 1,
            CatId = "abc123",
            Width = 800,
            Height = 600,
            ImageUrl = "https://example.com/cat.jpg",
            Tags = new List<TagEntity>
            {
                new() { Name = "Playful" },
                new() { Name = "Friendly" }
            }
        };

        var dto = CatMapper.ToDto(entity);

        Assert.Equal(1, dto.Id);
        Assert.Equal("abc123", dto.CatId);
        Assert.Equal(800, dto.Width);
        Assert.Equal(600, dto.Height);
        Assert.Equal("https://example.com/cat.jpg", dto.ImageUrl);
        Assert.Contains("Playful", dto.Tags);
        Assert.Contains("Friendly", dto.Tags);
    }

    [Fact]
    public void ToDto_HandlesNullTags()
    {
        var entity = new CatEntity { Id = 1, CatId = "abc", Tags = null! };

        var dto = CatMapper.ToDto(entity);

        Assert.Empty(dto.Tags);
    }

    [Fact]
    public void ToDtoList_MapsMultipleEntities()
    {
        var entities = new List<CatEntity>
        {
            new() { Id = 1, CatId = "a", Tags = [] },
            new() { Id = 2, CatId = "b", Tags = [] }
        };

        var dtos = CatMapper.ToDtoList(entities);

        Assert.Equal(2, dtos.Count);
        Assert.Equal("a", dtos[0].CatId);
        Assert.Equal("b", dtos[1].CatId);
    }
}
