using TentacleOracle.Models;

namespace TentacleOracle.BusinessLogic;

public interface IMealPlanGenerator
{
    IReadOnlyList<Dish> Generate(IReadOnlyList<Dish> dishes, int count = AppSettings.DefaultMealCount);
}