using Microsoft.AspNetCore.Mvc;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Requests;
using StealAllTheCats.Services.Interfaces;

namespace StealAllTheCats.Controllers;

[ApiController]
[Route("api/cats")]
public class CatsController(ICatService catService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCats([FromQuery] GetCatsRequest request, CancellationToken ct)
    {
        var result = await catService.GetCatsPaginatedAsync(request, ct);
        return ToResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCat(int id, CancellationToken ct)
    {
        var result = await catService.GetCatByIdAsync(id, ct);
        return ToResponse(result);
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetCatImage(int id, CancellationToken ct)
    {
        var imageData = await catService.GetCatImageAsync(id, ct);

        if (imageData == null || imageData.Length == 0)
            return NotFound("Image not found.");

        return File(imageData, "image/jpeg");
    }

    private IActionResult ToResponse<T>(Result<T> result)
    {
        if (!result.Success)
            return StatusCode(result.ErrorCode, new { error = result.ErrorMessage });

        return Ok(result.Data);
    }
}
