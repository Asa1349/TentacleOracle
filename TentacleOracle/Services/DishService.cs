using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;
using TentacleOracle.Models;

namespace TentacleOracle.Services;

public class DishService : IDishService
{
    private readonly AppDbContext _db;

    public DishService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Dish>> GetDishesAsync() =>
        await _db.Dishes.ToListAsync();

    public async Task<Dish?> GetDishByIdAsync(int id)
        => await _db.Dishes.FindAsync(id);

    public async Task AddDishAsync(Dish dish)
    {
        _db.Dishes.Add(dish);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Prevent a later retry from persisting the failed insert as well.
            _db.Entry(dish).State = EntityState.Detached;
            throw;
        }
    }

    public async Task UpdateDishAsync(Dish dish)
    {
        var existing = await _db.Dishes.FindAsync(dish.Id)
                       ?? throw new InvalidOperationException($"Dish {dish.Id} no longer exists.");
        var entry = _db.Entry(existing);
        var previousValues = entry.CurrentValues.Clone();

        try
        {
            entry.CurrentValues.SetValues(dish);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Keep failed edits out of the UI and subsequent saves.
            entry.CurrentValues.SetValues(previousValues);
            entry.State = EntityState.Unchanged;
            throw;
        }
    }

    public async Task DeleteDishAsync(int id)
    {
        var dish = await _db.Dishes.FindAsync(id);
        if (dish != null)
        {
            _db.Dishes.Remove(dish);
            await _db.SaveChangesAsync();
        }
    }
}