namespace StealAllTheCats.Dtos.Results;

public class ImportJobDto
{
    public int Id { get; set; }
    public string Status { get; set; } = "";
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public string? Message { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Completed { get; set; }
}
