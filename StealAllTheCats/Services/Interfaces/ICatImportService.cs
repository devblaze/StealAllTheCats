using StealAllTheCats.Dtos.Results;

namespace StealAllTheCats.Services.Interfaces;

public interface ICatImportService
{
    Task<ImportJobDto> StartImportAsync();
    Task<ImportJobDto?> GetImportStatusAsync(int jobId, CancellationToken ct);
    Task ProcessImportAsync(int jobId, CancellationToken ct);
}
