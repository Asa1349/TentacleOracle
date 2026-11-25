using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;
using TentacleOracle.Models;

namespace TentacleOracle.Services;

public sealed class DishService(IDbContextFactory<AppDbContext> contextFactory) : IDishService
{
    public async Task<List<Dish>> GetDishesAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Dishes.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddDishAsync(Dish dish, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Dishes.Add(dish);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDishAsync(Dish dish, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.Dishes.FindAsync([dish.Id], cancellationToken)
                       ?? throw new InvalidOperationException($"Dish {dish.Id} no longer exists.");
        db.Entry(existing).CurrentValues.SetValues(dish);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDishAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var dish = await db.Dishes.FindAsync([id], cancellationToken);
        if (dish != null)
        {
            db.Dishes.Remove(dish);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}