using StealAllTheCats.Database.Models;
using StealAllTheCats.Dtos.Mappers;
using Xunit;

namespace StealAllTheCats.Tests.Mappers;

public class ImportJobMapperTests
{
    [Fact]
    public void ToDto_MapsAllFields()
    {
        var entity = new ImportJobEntity
        {
            Id = 5,
            Status = ImportJobStatus.Completed,
            Imported = 20,
            Skipped = 3,
            Failed = 2,
            Message = "Done",
            Created = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Completed = new DateTime(2025, 1, 1, 0, 1, 0, DateTimeKind.Utc)
        };

        var dto = ImportJobMapper.ToDto(entity);

        Assert.Equal(5, dto.Id);
        Assert.Equal("completed", dto.Status);
        Assert.Equal(20, dto.Imported);
        Assert.Equal(3, dto.Skipped);
        Assert.Equal(2, dto.Failed);
        Assert.Equal("Done", dto.Message);
        Assert.NotNull(dto.Completed);
    }

    [Fact]
    public void ToDto_StatusIsLowercase()
    {
        var entity = new ImportJobEntity { Status = ImportJobStatus.Running };

        var dto = ImportJobMapper.ToDto(entity);

        Assert.Equal("running", dto.Status);
    }
}
