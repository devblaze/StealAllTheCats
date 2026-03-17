using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Database.Models;

namespace StealAllTheCats.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<CatEntity> Cats => Set<CatEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CatEntity>()
            .HasMany(c => c.Tags)
            .WithMany(t => t.Cats);

        base.OnModelCreating(builder);
    }
}