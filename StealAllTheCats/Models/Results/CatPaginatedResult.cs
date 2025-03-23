namespace StealAllTheCats.Models.Results;

public class CatPaginatedResult
{
    public int TotalItems { get; set; }
    public List<CatEntity>? Data { get; set; }
}