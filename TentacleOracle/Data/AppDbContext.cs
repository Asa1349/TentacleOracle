using Microsoft.EntityFrameworkCore;
using TentacleOracle.Models;
using AppSettingsModel = TentacleOracle.Models.AppSettings;

namespace TentacleOracle.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Dish> Dishes { get; set; }

    public DbSet<AppSettings> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.Property(settings => settings.Id).ValueGeneratedNever();
            entity.Property(settings => settings.MealCount).HasDefaultValue(AppSettingsModel.DefaultMealCount);
            entity.HasData(new AppSettingsModel { Id = 1 });
        });
    }
}