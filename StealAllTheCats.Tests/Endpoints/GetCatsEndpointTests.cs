using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Endpoints;
using StealAllTheCats.Models;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;
using Xunit;

namespace StealAllTheCats.Tests.Endpoints;

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
            new CatEntity
            {
                CatId = "cat1",
                Image = new byte[] { 0x01, 0x02 }, Tags = [new TagEntity { Name = "Playful" }]
            },
            new CatEntity
            {
                CatId = "cat2",
                Image = new byte[] { 0x03, 0x04 }, Tags = [new TagEntity { Name = "Sleepy" }]
            },
            new CatEntity
            {
                CatId = "cat3",
                Image = new byte[] { 0x05 }, Tags = [new TagEntity { Name = "Playful" }]
            }
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var ep = Factory.Create<GetCatsEndpoint>(context);

        await ep.HandleAsync(new GetCatsPaginatedRequest { Tag = "Playful", Page = 1, PageSize = 10 }, CancellationToken.None);
        
        var response = ep.Response as CatPaginatedResponse;
        
        Assert.NotNull(response);
        
        Assert.Equal(2, response.TotalItems);
        Assert.All(response.Data, c =>
        {
            if (c.Tags != null)
            {
                Assert.Contains(c.Tags, t => t.Name == "Playful");
            }
        });
    }
}
