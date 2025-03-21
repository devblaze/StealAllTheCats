using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Endpoints;
using StealAllTheCats.Models;
using StealAllTheCats.Services;

namespace StealAllCats.Tests.Endpoints;

public class FetchCatsEndpointTests
{
    [Fact]
    public async Task FetchCats_ShouldAddCatsToDatabase_WithoutDuplicates()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FetchCatsTest")
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.Cats.Add(new CatEntity { CatId = "abc123" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var mockCatService = new Mock<CatService>(new HttpClient());
        mockCatService.Setup(svc => svc.FetchCatsAsync(25))
            .ReturnsAsync(new List<CatEntity>
            {
                new() { CatId = "abc123" },
                new() { CatId = "def456" }
            });

        var ep = Factory.Create<FetchCatsEndpoint>(context, mockCatService.Object);
        await ep.HandleAsync(CancellationToken.None);

        var cats = context.Cats.ToList();
        Assert.Equal(2, cats.Count); // One added initially, one added via service call
    }
}