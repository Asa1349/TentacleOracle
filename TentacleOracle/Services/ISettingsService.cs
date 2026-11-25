namespace TentacleOracle.Services;

public interface ISettingsService
{
    Task<int> GetMealCountAsync();
    Task SaveMealCountAsync(int count);
}