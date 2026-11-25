using Microsoft.EntityFrameworkCore;
using TentacleOracle.Models;

namespace TentacleOracle.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Dish> Dishes { get; set; }

    public DbSet<AppSettings> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.Property(settings => settings.Id).ValueGeneratedNever();
            entity.Property(settings => settings.MealCount).HasDefaultValue(7);
            entity.HasData(new AppSettings { Id = 1, MealCount = 7 });
        });
    }
}