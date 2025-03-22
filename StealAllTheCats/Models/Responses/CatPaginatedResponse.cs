namespace StealAllTheCats.Models.Responses;

public class CatPaginatedResponse
{
    public int TotalItems { get; set; }
    public List<CatEntity>? Data { get; set; }
}