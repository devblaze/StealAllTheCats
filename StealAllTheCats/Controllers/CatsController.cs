using Microsoft.AspNetCore.Mvc;
using StealAllTheCats.Dtos.Requets;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatsController(ICatService catService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCats([FromQuery] GetCatsRequest request, CancellationToken ct)
    {
        var result = await catService.GetCatsPaginatedAsync(request, ct);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCat(int id, CancellationToken ct)
    {
        var result = await catService.GetCatByIdAsync(id, ct);

        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> FetchCats(CancellationToken ct, int catImages = 25)
    {
        var result = await catService.FetchCatsAsync(catImages);
        if (!result.Success)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, result.ErrorMessage);

        return Ok(result.Data);
    }
}