namespace StealAllTheCats.Dtos.Results;

public class CatPaginatedResult
{
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CatDto> Cats { get; set; } = [];
}
