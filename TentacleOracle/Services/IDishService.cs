using FoodOracle.Models;

namespace TentacleOracle.Services;

public interface IDishService
{
    Task<List<Dish>> GetDishesAsync();
    Task<Dish?> GetDishByIdAsync(int id);
    Task AddDishAsync(Dish dish);
    Task UpdateDishAsync(Dish dish);
    Task DeleteDishAsync(int id);
}