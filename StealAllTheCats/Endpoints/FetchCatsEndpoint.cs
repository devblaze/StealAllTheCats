using FastEndpoints;
using StealAllTheCats.Data;
using StealAllTheCats.Services;

namespace StealAllTheCats.Endpoints;

public class FetchCatsEndpoint : EndpointWithoutRequest
{
    private readonly ApplicationDbContext _db;
    private readonly CatService _catService;

    public FetchCatsEndpoint(ApplicationDbContext db, CatService catService)
    {
        _db = db;
        _catService = catService;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/api/cats/fetch");
        Description(sb => sb.WithSummary("Fetch 25 cat images from CaaS API and store them"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var newCats = await _catService.FetchCatsAsync(25);
        var existingCatIds = _db.Cats.Select(c => c.CatId).ToHashSet();

        foreach (var cat in newCats)
        {
            if (!existingCatIds.Contains(cat.CatId))
                _db.Cats.Add(cat);
        }

        await _db.SaveChangesAsync(ct);
        await SendOkAsync(new { Message = "Fetched and saved 25 cats!", Count = newCats.Count }, ct);
    }
}