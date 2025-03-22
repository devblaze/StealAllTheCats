using StealAllTheCats.Models;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;

namespace StealAllTheCats.Endpoints;

public class GetCatsEndpoint(ApplicationDbContext db) : Endpoint<GetCatsRequest>
{
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/cats");
        Description(sb => sb.WithSummary("Retrieve all cats with pagination and optional tag filtering"));
    }

    public override async Task HandleAsync(GetCatsRequest req, CancellationToken ct)
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

public class GetCatsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Tag { get; set; }
}