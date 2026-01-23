using Microsoft.EntityFrameworkCore;
using TentacleOracle.Models;

namespace TentacleOracle.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    
    public DbSet<Dish> Dishes { get; set; }
    
}