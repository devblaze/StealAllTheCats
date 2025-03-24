using System.Text.Json.Serialization;

namespace StealAllTheCats.Database.Models;

public class TagEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public ICollection<CatEntity> Cats { get; set; } = new List<CatEntity>();
}