using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;
using TentacleOracle.Models;

namespace TentacleOracle.Services;

public class DishService : IDishService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DishService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Dish>> GetDishesAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Dishes.AsNoTracking().ToListAsync();
    }

    public async Task<Dish?> GetDishByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Dishes.AsNoTracking().SingleOrDefaultAsync(dish => dish.Id == id);
    }

    public async Task AddDishAsync(Dish dish)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Dishes.Add(dish);
        await db.SaveChangesAsync();
    }

    public async Task UpdateDishAsync(Dish dish)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.Dishes.FindAsync(dish.Id)
                       ?? throw new InvalidOperationException($"Dish {dish.Id} no longer exists.");
        db.Entry(existing).CurrentValues.SetValues(dish);
        await db.SaveChangesAsync();
    }

    public async Task DeleteDishAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var dish = await db.Dishes.FindAsync(id);
        if (dish != null)
        {
            db.Dishes.Remove(dish);
            await db.SaveChangesAsync();
        }
    }
}