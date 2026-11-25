using TentacleOracle.Models;

namespace TentacleOracle.BusinessLogic;

public sealed class MealPlanGenerator : IMealPlanGenerator
{
    public IReadOnlyList<Dish> Generate(IReadOnlyList<Dish> dishes, int count = AppSettings.DefaultMealCount)
    {
        ArgumentNullException.ThrowIfNull(dishes);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var candidates = dishes.ToArray();
        Random.Shared.Shuffle(candidates);
        return candidates.Take(count).ToArray();
    }
}