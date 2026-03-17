using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Database.Models;

namespace StealAllTheCats.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<CatEntity> Cats => Set<CatEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<ImportJobEntity> ImportJobs => Set<ImportJobEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CatEntity>()
            .HasMany(c => c.Tags)
            .WithMany(t => t.Cats);

        builder.Entity<CatEntity>()
            .HasIndex(c => c.CatId)
            .IsUnique();

        builder.Entity<TagEntity>()
            .HasIndex(t => t.Name)
            .IsUnique();

        base.OnModelCreating(builder);
    }
}
