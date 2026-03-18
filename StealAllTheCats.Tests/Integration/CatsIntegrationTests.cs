using System.Net;
using System.Net.Http.Json;
using StealAllTheCats.Dtos.Results;
using Xunit;

namespace StealAllTheCats.Tests.Integration;

public class CatsIntegrationTests : IClassFixture<CatCollectorFactory>
{
    private readonly HttpClient _client;

    public CatsIntegrationTests(CatCollectorFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCats_ReturnsOkWithPaginatedStructure()
    {
        var response = await _client.GetAsync("/api/cats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CatPaginatedResult>();
        Assert.NotNull(result);
        Assert.True(result!.TotalItems >= 0);
        Assert.NotNull(result.Cats);
    }

    [Fact]
    public async Task GetCats_ReturnsCatsAfterImport()
    {
        await ImportAndWait();

        var response = await _client.GetAsync("/api/cats");
        var result = await response.Content.ReadFromJsonAsync<CatPaginatedResult>();

        Assert.True(result!.TotalItems > 0);
        Assert.NotEmpty(result.Cats);

        var cat = result.Cats.First();
        Assert.False(string.IsNullOrEmpty(cat.CatId));
        Assert.NotEmpty(cat.Tags);
    }

    [Fact]
    public async Task GetCats_PaginationWorks()
    {
        await ImportAndWait();

        var response = await _client.GetAsync("/api/cats?page=1&pageSize=2");
        var result = await response.Content.ReadFromJsonAsync<CatPaginatedResult>();

        Assert.Equal(1, result!.Page);
        Assert.Equal(2, result.PageSize);
        Assert.True(result.Cats.Count <= 2);
    }

    [Fact]
    public async Task GetCats_FilterByTagWorks()
    {
        await ImportAndWait();

        var response = await _client.GetAsync("/api/cats?tag=Playful");
        var result = await response.Content.ReadFromJsonAsync<CatPaginatedResult>();

        Assert.NotNull(result);
        Assert.True(result!.TotalItems > 0);
        foreach (var cat in result.Cats)
            Assert.Contains("Playful", cat.Tags);
    }

    [Fact]
    public async Task GetCats_FilterByNonexistentTagReturnsEmpty()
    {
        await ImportAndWait();

        var response = await _client.GetAsync("/api/cats?tag=Nonexistent");
        var result = await response.Content.ReadFromJsonAsync<CatPaginatedResult>();

        Assert.Equal(0, result!.TotalItems);
    }

    [Fact]
    public async Task GetCatById_ReturnsCatWithTags()
    {
        await ImportAndWait();

        // Get the first cat's ID
        var listResponse = await _client.GetAsync("/api/cats");
        var list = await listResponse.Content.ReadFromJsonAsync<CatPaginatedResult>();
        var firstId = list!.Cats.First().Id;

        var response = await _client.GetAsync($"/api/cats/{firstId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cat = await response.Content.ReadFromJsonAsync<CatDto>();
        Assert.NotNull(cat);
        Assert.Equal(firstId, cat!.Id);
        Assert.NotEmpty(cat.Tags);
    }

    [Fact]
    public async Task GetCatById_Returns404ForNonexistent()
    {
        var response = await _client.GetAsync("/api/cats/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCatImage_ReturnsImageBytes()
    {
        await ImportAndWait();

        var listResponse = await _client.GetAsync("/api/cats");
        var list = await listResponse.Content.ReadFromJsonAsync<CatPaginatedResult>();
        var firstId = list!.Cats.First().Id;

        var response = await _client.GetAsync($"/api/cats/{firstId}/image");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/jpeg", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task GetCatImage_Returns404ForNonexistent()
    {
        var response = await _client.GetAsync("/api/cats/99999/image");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCats_InvalidPageReturnsValidationError()
    {
        var response = await _client.GetAsync("/api/cats?page=0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCats_InvalidPageSizeReturnsValidationError()
    {
        var response = await _client.GetAsync("/api/cats?pageSize=0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task ImportAndWait()
    {
        var post = await _client.PostAsync("/api/cat-imports", null);
        var job = await post.Content.ReadFromJsonAsync<ImportJobResponse>();
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(500);
            var status = await _client.GetAsync($"/api/cat-imports/{job!.Id}");
            var dto = await status.Content.ReadFromJsonAsync<ImportJobDto>();
            if (dto!.Status is "completed" or "failed") break;
        }
    }

    private record ImportJobResponse(int Id, string Status);
}
