using Moq;
using Moq.Protected;
using StealAllTheCats.Services;
using System.Net;
using System.Text.Json;

namespace StealAllCats.Tests.Services;

public class CatServiceTests
{
    [Fact]
    public async Task FetchCatsAsync_ShouldReturnCorrectNumberOfCats()
    {
        var fakeHttpMessageHandler = new Mock<HttpMessageHandler>();
        fakeHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new[]
                {
                    new { Id = "abc123", Url = "https://caturl.com/img1.jpg", Width = 500, Height = 500, Breeds = new[]{ new { Temperament = "Playful, Curious" } } },
                    new { Id = "def456", Url = "https://caturl.com/img2.jpg", Width = 600, Height = 400, Breeds = new[]{ new { Temperament = "Friendly, Loyal" } } },
                })),
            });

        var client = new HttpClient(fakeHttpMessageHandler.Object) { BaseAddress = new Uri("https://api.thecatapi.com/v1/") };
        var service = new CatService(client);

        var results = await service.FetchCatsAsync(limit: 2);

        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Contains(results, cat => cat.CatId == "abc123");
        Assert.Contains(results, cat => cat.CatId == "def456");
    }
}