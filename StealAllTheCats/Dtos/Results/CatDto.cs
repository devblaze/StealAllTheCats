namespace StealAllTheCats.Dtos.Results;

public class CatDto
{
    public int Id { get; set; }
    public string CatId { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public string ImageUrl { get; set; } = "";
    public IEnumerable<string> Tags { get; set; } = [];
}