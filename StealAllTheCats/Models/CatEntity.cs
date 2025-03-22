namespace StealAllTheCats.Models;

public class CatEntity
{
    public int Id { get; set; }
    public string CatId { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] Image { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public ICollection<TagEntity>? Tags { get; set; } = new List<TagEntity>();
}