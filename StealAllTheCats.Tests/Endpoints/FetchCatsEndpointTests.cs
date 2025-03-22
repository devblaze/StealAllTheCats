using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Moq;
using StealAllTheCats.Data;
using StealAllTheCats.Endpoints;
using StealAllTheCats.Models;
using StealAllTheCats.Services;
using Xunit;

namespace StealAllTheCats.Tests.Endpoints;

public class FetchCatsEndpointTests
{
    [Fact]
    public async Task FetchCats_ShouldAddCatsToDatabase_WithoutDuplicates()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("FetchCatsTest")
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.Cats.Add(new CatEntity
        {
            CatId = "abc123",
            Image = new byte[] { 0x1, 0x2, 0x3 } // Provide dummy byte data for test
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var mockCatService = new Mock<ICatService>();
        mockCatService.Setup(svc => svc.FetchCatsAsync(25))
            .ReturnsAsync(new List<CatEntity>
            {
                new CatEntity { CatId = "abc123", Image = new byte[] { 0x01 } },
                new CatEntity { CatId = "def456", Image = new byte[] { 0x02 } }
            });

        var ep = Factory.Create<FetchCatsEndpoint>(context, mockCatService.Object);
        await ep.HandleAsync(CancellationToken.None);

        var cats = context.Cats.ToList();

        Assert.Equal(2, cats.Count);
        Assert.Contains(cats, c => c.CatId == "abc123");
        Assert.Contains(cats, c => c.CatId == "def456");
    }
}