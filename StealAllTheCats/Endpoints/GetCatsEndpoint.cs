using StealAllTheCats.Models;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Models.Responses;
using StealAllTheCats.Services;

namespace StealAllTheCats.Endpoints;

public class GetCatsEndpoint(ApplicationDbContext db, ICatService catService) : Endpoint<GetCatsPaginatedRequest>
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
        await SendOkAsync(await catService.GetCatsPaginatedAsync(req, ct), ct);
    }
}