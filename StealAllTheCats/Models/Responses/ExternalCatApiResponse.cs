namespace StealAllTheCats.Models.Responses;

public class ExternalCatApiResponse
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }

    public List<CatBreed> Breeds { get; set; } = new();
}

public class CatBreed
{
    public string? Temperament { get; set; }
}