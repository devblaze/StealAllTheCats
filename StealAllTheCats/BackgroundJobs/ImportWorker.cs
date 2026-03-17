using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.BackgroundJobs;

public class ImportWorker(
    ImportQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<ImportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Import worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var jobId = await queue.DequeueAsync(stoppingToken);
                logger.LogInformation("Processing import job {JobId}", jobId);

                using var scope = scopeFactory.CreateScope();
                var importService = scope.ServiceProvider.GetRequiredService<ICatImportService>();
                await importService.ProcessImportAsync(jobId, stoppingToken);

                logger.LogInformation("Import job {JobId} completed", jobId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing import job");
            }
        }
    }
}
