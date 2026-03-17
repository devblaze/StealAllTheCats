using Microsoft.AspNetCore.Mvc;
using StealAllTheCats.Models.Requets;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatsController(ICatService catService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCats([FromQuery] GetCatsRequest request, CancellationToken ct)
    {
        return Ok(await catService.GetCatsPaginatedAsync(request, ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCat([FromQuery] GetCatsRequest request, CancellationToken ct)
    {
        var result = await catService.GetCatByIdAsync(request, ct);

        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> FetchCats(CancellationToken ct, int catImages = 25)
    {
        var result = await catService.FetchCatsAsync(catImages);
        return Ok(result);
    }

    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetCatImage(int id, CancellationToken ct)
    {
        var cat = await catService.GetCatByIdAsync(new GetCatsRequest { Id = id }, ct);

        if (cat == null || cat.Image == null || cat.Image.Length == 0)
            return NotFound("No image found.");
        
        return File(cat.Image, "image/jpeg");
    }
}