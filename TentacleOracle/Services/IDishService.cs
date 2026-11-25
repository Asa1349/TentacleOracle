using TentacleOracle.Models;

namespace TentacleOracle.Services;

public interface IDishService
{
    Task<List<Dish>> GetDishesAsync(CancellationToken cancellationToken = default);
    Task AddDishAsync(Dish dish, CancellationToken cancellationToken = default);
    Task UpdateDishAsync(Dish dish, CancellationToken cancellationToken = default);
    Task DeleteDishAsync(int id, CancellationToken cancellationToken = default);
}