using System.Net;
using Xunit;

namespace StealAllTheCats.Tests.Integration;

public class SwaggerIntegrationTests : IClassFixture<CatCollectorFactory>
{
    private readonly HttpClient _client;

    public SwaggerIntegrationTests(CatCollectorFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerUI_IsAccessible()
    {
        var response = await _client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_IsAccessible()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("cat-imports", content);
        Assert.Contains("cats", content);
    }
}
