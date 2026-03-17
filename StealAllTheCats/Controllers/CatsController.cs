using Microsoft.AspNetCore.Mvc;
using StealAllTheCats.Dtos;
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
        
        return GetResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCat(int id, CancellationToken ct)
    {
        var result = await catService.GetCatByIdAsync(id, ct);

        return GetResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> FetchCats(CancellationToken ct, int catImages = 25)
    {
        var result = await catService.FetchCatsAsync(catImages);
        
        return GetResult(result);
    }

    private IActionResult GetResult<T>(Result<T> result)
    {
        if (!result.Success)
            return StatusCode(result.ErrorCode, result.ErrorMessage!);

        return Ok(result.Data);
    }
    
}