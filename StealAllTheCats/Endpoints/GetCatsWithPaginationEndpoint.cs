using StealAllTheCats.Models;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models.Requets;

namespace StealAllTheCats.Endpoints;

public class GetCatsEndpoint(ApplicationDbContext db) : Endpoint<GetCatsPaginatedRequest>
{
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/cats");
        Description(sb => sb.WithSummary("Retrieve all cats with pagination and optional tag filtering"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCatsPaginatedRequest req, CancellationToken ct)
    {
        var query = db.Cats.Include(c => c.Tags).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Tag))
            query = query.Where(c => c.Tags != null && c.Tags.Any(t => t.Name == req.Tag));

        var total = await query.CountAsync(ct);
        var data = await query.Skip((req.Page - 1) * req.PageSize)
                              .Take(req.PageSize)
                              .ToListAsync(ct);

        await SendOkAsync(new { TotalItems = total, Data = data }, ct);
    }
}