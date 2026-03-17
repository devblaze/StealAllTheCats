using StealAllTheCats.Models;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;

namespace StealAllTheCats.Endpoints;

public class GetCatEndpoint : Endpoint<GetCatRequest>
{
    private readonly ApplicationDbContext _db;

    public GetCatEndpoint(ApplicationDbContext db) => _db = db;

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/cats/{Id}");
        Description(sb => sb.WithSummary("Retrieve cat by ID"));
    }

    public override async Task HandleAsync(GetCatRequest req, CancellationToken ct)
    {
        var cat = await _db.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == req.Id, ct);
        if (cat is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        await SendOkAsync(cat, ct);
    }
}

public class GetCatRequest
{
    public int Id { get; set; }
}