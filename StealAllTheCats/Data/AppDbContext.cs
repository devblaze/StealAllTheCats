using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Models;

namespace StealAllTheCats.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

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