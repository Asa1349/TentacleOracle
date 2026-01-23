using FoodOracle.Models;
using Microsoft.EntityFrameworkCore;

namespace TentacleOracle.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    
    public DbSet<Dish> Dishes { get; set; }
    
}