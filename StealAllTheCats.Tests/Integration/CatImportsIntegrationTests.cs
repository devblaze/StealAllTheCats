using System.Net;
using System.Net.Http.Json;
using StealAllTheCats.Dtos.Results;
using Xunit;

namespace StealAllTheCats.Tests.Integration;

public class CatImportsIntegrationTests : IClassFixture<CatCollectorFactory>
{
    private readonly HttpClient _client;

    public CatImportsIntegrationTests(CatCollectorFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCatImports_Returns202WithJobId()
    {
        var response = await _client.PostAsync("/api/cat-imports", null);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ImportJobResponse>();
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);
        Assert.Equal("queued", body.Status);
    }

    [Fact]
    public async Task GetCatImportStatus_ReturnsJobAfterCreation()
    {
        var postResponse = await _client.PostAsync("/api/cat-imports", null);
        var created = await postResponse.Content.ReadFromJsonAsync<ImportJobResponse>();

        var response = await _client.GetAsync($"/api/cat-imports/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var job = await response.Content.ReadFromJsonAsync<ImportJobDto>();
        Assert.NotNull(job);
        Assert.Equal(created.Id, job!.Id);
    }

    [Fact]
    public async Task GetCatImportStatus_Returns404ForNonexistent()
    {
        var response = await _client.GetAsync("/api/cat-imports/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ImportJob_CompletesAndImportsCats()
    {
        var postResponse = await _client.PostAsync("/api/cat-imports", null);
        var created = await postResponse.Content.ReadFromJsonAsync<ImportJobResponse>();

        // Poll until completed or timeout
        ImportJobDto? job = null;
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(500);
            var statusResponse = await _client.GetAsync($"/api/cat-imports/{created!.Id}");
            job = await statusResponse.Content.ReadFromJsonAsync<ImportJobDto>();
            if (job!.Status is "completed" or "failed")
                break;
        }

        Assert.NotNull(job);
        Assert.Equal("completed", job!.Status);
        Assert.Equal(3, job.Imported);
        Assert.Equal(0, job.Skipped);

        // Verify cats are now in the database
        var catsResponse = await _client.GetAsync("/api/cats");
        Assert.Equal(HttpStatusCode.OK, catsResponse.StatusCode);
        var cats = await catsResponse.Content.ReadFromJsonAsync<CatPaginatedResult>();
        Assert.Equal(3, cats!.TotalItems);
    }

    [Fact]
    public async Task ImportJob_SkipsDuplicatesOnSecondRun()
    {
        // First import
        var post1 = await _client.PostAsync("/api/cat-imports", null);
        var job1 = await post1.Content.ReadFromJsonAsync<ImportJobResponse>();
        await WaitForCompletion(job1!.Id);

        // Second import (same cats)
        var post2 = await _client.PostAsync("/api/cat-imports", null);
        var job2 = await post2.Content.ReadFromJsonAsync<ImportJobResponse>();
        var finalJob = await WaitForCompletion(job2!.Id);

        Assert.Equal("completed", finalJob!.Status);
        Assert.Equal(0, finalJob.Imported);
        Assert.Equal(3, finalJob.Skipped);
    }

    private async Task<ImportJobDto?> WaitForCompletion(int jobId)
    {
        ImportJobDto? job = null;
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(500);
            var response = await _client.GetAsync($"/api/cat-imports/{jobId}");
            job = await response.Content.ReadFromJsonAsync<ImportJobDto>();
            if (job!.Status is "completed" or "failed")
                break;
        }
        return job;
    }

    private record ImportJobResponse(int Id, string Status);
}
