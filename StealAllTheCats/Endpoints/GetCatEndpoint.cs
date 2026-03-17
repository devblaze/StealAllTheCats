using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Services;

namespace StealAllTheCats.Endpoints;

public class GetCatEndpoint(ApplicationDbContext db) : Endpoint<GetCatRequest>
{
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/cats/{Id}");
        Description(sb => sb.WithSummary("Retrieve cat by ID"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCatRequest req, CancellationToken ct)
    {
        var cat = await db.Cats.Include(c => c.Tags).FirstOrDefaultAsync(c => c.Id == req.Id, ct);
        if (cat is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        await SendOkAsync(cat, ct);
    }
}