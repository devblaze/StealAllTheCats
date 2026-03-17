using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos.Results;

namespace StealAllTheCats.Dtos.Mappers;

public static class ImportJobMapper
{
    public static ImportJobDto ToDto(ImportJobEntity entity)
    {
        return new ImportJobDto
        {
            Id = entity.Id,
            Status = entity.Status.ToString().ToLowerInvariant(),
            Imported = entity.Imported,
            Skipped = entity.Skipped,
            Failed = entity.Failed,
            Message = entity.Message,
            Created = entity.Created,
            Completed = entity.Completed
        };
    }
}
