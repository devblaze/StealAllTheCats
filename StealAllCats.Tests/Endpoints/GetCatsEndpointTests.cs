using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Endpoints;
using StealAllTheCats.Models;
using Xunit;

namespace StealAllCats.Tests.Endpoints;

public class GetCatsEndpointTests
{
    [Fact]
    public async Task GetCats_WithPagination_AndTagFiltering_WorksCorrectly()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "PagingTest")
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.Cats.AddRange(
            new CatEntity { CatId = "cat1", Tags = new[] { new TagEntity { Name = "Playful" } } },
            new CatEntity { CatId = "cat2", Tags = new[] { new TagEntity { Name = "Sleepy" } } },
            new CatEntity { CatId = "cat3", Tags = new[] { new TagEntity { Name = "Playful" } } }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var ep = Factory.Create<GetCatsEndpoint>(context);

        await ep.HandleAsync(new GetCatsRequest { Tag = "Playful", Page = 1, PageSize = 10 }, CancellationToken.None);
        
        var response = ep.Response as GetCatsResponse;

        Assert.NotNull(response);

        Assert.Equal(2, response.TotalItems);
        Assert.All(response.Data, c => Assert.Contains(c.Tags, t => t.Name == "Playful"));
    }
}

public record GetCatsResponse(int TotalItems, List<CatEntity> Data);
