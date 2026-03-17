namespace StealAllTheCats.Database.Models;

public class ImportJobEntity
{
    public int Id { get; set; }
    public ImportJobStatus Status { get; set; } = ImportJobStatus.Queued;
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public string? Message { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Completed { get; set; }
}

public enum ImportJobStatus
{
    Queued,
    Running,
    Completed,
    Failed
}
