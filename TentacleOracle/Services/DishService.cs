using TentacleOracle.Models;
using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;

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
            // Ein erneuter Versuch darf den fehlgeschlagenen Insert nicht zusätzlich speichern.
            _db.Entry(dish).State = EntityState.Detached;
            throw;
        }
    }

    public async Task UpdateDishAsync(Dish dish)
    {
        _db.Dishes.Update(dish);
        await _db.SaveChangesAsync();
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