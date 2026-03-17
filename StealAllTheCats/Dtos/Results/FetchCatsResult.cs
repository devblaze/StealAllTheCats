using StealAllTheCats.Models;

namespace StealAllTheCats.Dtos.Results;

public class FetchCatsResult
{
    public int NewCatsCount { get; set; }
    public int DuplicateCatsCount { get; set; }
    public List<CatDto> Cats { get; set; } = [];
}