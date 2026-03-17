using FastEndpoints;
using StealAllTheCats.Data;
using StealAllTheCats.Services;

namespace StealAllTheCats.Endpoints;

public class FetchCatsEndpoint(ApplicationDbContext db, ICatService catService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/api/cats/fetch");
        Description(sb => sb.WithSummary("Fetch 25 cat images from CaaS API and store them"));
        AllowAnonymous(); // include only if your setup or middleware expects explicit anonymous allowance
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var newCats = await catService.FetchCatsAsync(25);
        var existingCatIds = db.Cats.Select(c => c.CatId).ToHashSet();

        foreach (var cat in newCats)
        {
            if (!existingCatIds.Contains(cat.CatId))
                db.Cats.Add(cat);
        }

        await db.SaveChangesAsync(ct);
        await SendOkAsync(new { Message = "Fetched and saved 25 cats!", Count = newCats.Count }, ct);
    }
}