namespace StealAllTheCats.Models;

public class CatEntity
{
    public int Id { get; set; }
    public string CatId { get; set; } = default!;
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] Image { get; set; } = default!;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public ICollection<TagEntity>? Tags { get; set; } = new List<TagEntity>();
}