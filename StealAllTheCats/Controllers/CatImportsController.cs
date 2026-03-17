using Microsoft.AspNetCore.Mvc;
using StealAllTheCats.BackgroundJobs;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Controllers;

[ApiController]
[Route("api/cat-imports")]
public class CatImportsController(
    ICatImportService importService,
    ImportQueue importQueue) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> StartImport()
    {
        var job = await importService.StartImportAsync();
        await importQueue.EnqueueAsync(job.Id);

        return Accepted(new { job.Id, job.Status });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetImportStatus(int id, CancellationToken ct)
    {
        var job = await importService.GetImportStatusAsync(id, ct);

        if (job == null)
            return NotFound(new { error = "Import job not found." });

        return Ok(job);
    }
}
