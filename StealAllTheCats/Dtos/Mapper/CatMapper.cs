using StealAllTheCats.Models;

namespace StealAllTheCats.Dtos.Mapper;

public static class CatMapper
{
    public static CatDto ToDto(CatEntity entity)
    {
        return new CatDto
        {
            Id = entity.Id,
            CatId = entity.CatId,
            Width = entity.Width,
            Height = entity.Height,
            Tags = entity.Tags?.Select(tag => tag.Name) ?? []
        };
    }
    
    public static List<CatDto> ToDtoList(IEnumerable<CatEntity> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}