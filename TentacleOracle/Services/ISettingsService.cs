namespace TentacleOracle.Services;

public interface ISettingsService
{
    Task<int> GetMealCountAsync(CancellationToken cancellationToken = default);
    Task SaveMealCountAsync(int count, CancellationToken cancellationToken = default);
}