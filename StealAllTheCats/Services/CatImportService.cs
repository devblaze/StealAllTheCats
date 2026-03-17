using StealAllTheCats.Common;
using StealAllTheCats.Common.Dtos;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Mappers;
using StealAllTheCats.Dtos.Responses;
using StealAllTheCats.Dtos.Results;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Services;

public class CatImportService(
    IApiClient apiClient,
    IConfiguration configuration,
    IGenericRepository<CatEntity> catRepository,
    IGenericRepository<TagEntity> tagRepository,
    IGenericRepository<ImportJobEntity> jobRepository,
    ILogger<CatImportService> logger) : ICatImportService
{
    private readonly CatApiConfig _config = configuration.CatApiConfig();

    public async Task<ImportJobDto> StartImportAsync()
    {
        var job = new ImportJobEntity();
        await jobRepository.AddAsync(job);
        await jobRepository.SaveChangesAsync();
        return ImportJobMapper.ToDto(job);
    }

    public async Task<ImportJobDto?> GetImportStatusAsync(int jobId, CancellationToken ct)
    {
        var job = await jobRepository.GetByIdAsync(jobId);
        return job == null ? null : ImportJobMapper.ToDto(job);
    }

    public async Task ProcessImportAsync(int jobId, CancellationToken ct)
    {
        var job = await jobRepository.GetByIdAsync(jobId);
        if (job == null) return;

        job.Status = ImportJobStatus.Running;
        await jobRepository.SaveChangesAsync();

        try
        {
            var apiResult = await apiClient.GetAsync<List<ExternalCatApiResponse>>(
                $"images/search?limit=25&has_breeds=1&api_key={_config.ApiKey}");

            if (!apiResult.Success)
            {
                job.Status = ImportJobStatus.Failed;
                job.Message = apiResult.ErrorMessage;
                job.Completed = DateTime.UtcNow;
                await jobRepository.SaveChangesAsync();
                return;
            }

            foreach (var apiCat in apiResult.Data!)
            {
                ct.ThrowIfCancellationRequested();

                if (await catRepository.ExistsAsync(c => c.CatId == apiCat.Id))
                {
                    job.Skipped++;
                    continue;
                }

                try
                {
                    var tags = await GetOrCreateTags(
                        apiCat.Breeds
                            .SelectMany(b => b.Temperament?.Split(',') ?? [])
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .Distinct());

                    byte[]? imageBytes = null;
                    try
                    {
                        imageBytes = await apiClient.DownloadBytesAsync(apiCat.Url);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to download image for cat {CatId}", apiCat.Id);
                    }

                    var entity = new CatEntity
                    {
                        CatId = apiCat.Id,
                        Width = apiCat.Width,
                        Height = apiCat.Height,
                        ImageUrl = apiCat.Url,
                        ImageData = imageBytes,
                        Tags = tags
                    };

                    await catRepository.AddAsync(entity);
                    await catRepository.SaveChangesAsync();
                    job.Imported++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to import cat {CatId}", apiCat.Id);
                    job.Failed++;
                }
            }

            job.Status = ImportJobStatus.Completed;
            job.Message = $"Import finished. {job.Imported} imported, {job.Skipped} skipped, {job.Failed} failed.";
        }
        catch (OperationCanceledException)
        {
            job.Status = ImportJobStatus.Failed;
            job.Message = "Import was cancelled.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Import job {JobId} failed unexpectedly", jobId);
            job.Status = ImportJobStatus.Failed;
            job.Message = ex.Message;
        }

        job.Completed = DateTime.UtcNow;
        await jobRepository.SaveChangesAsync();
    }

    private async Task<List<TagEntity>> GetOrCreateTags(IEnumerable<string> tagNames)
    {
        var names = tagNames.ToList();
        var existingTags = (await tagRepository.FindAsync(tag => names.Contains(tag.Name))).ToList();
        var existingNames = existingTags.Select(t => t.Name).ToHashSet();

        var newTags = names
            .Where(n => !existingNames.Contains(n))
            .Select(n => new TagEntity { Name = n })
            .ToList();

        if (newTags.Count > 0)
        {
            await tagRepository.AddRangeAsync(newTags);
            await tagRepository.SaveChangesAsync();
            existingTags.AddRange(newTags);
        }

        return existingTags;
    }
}
