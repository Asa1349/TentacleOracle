using TentacleOracle.Enums;
using TentacleOracle.Models;

namespace TentacleOracle.BusinessLogic;

public static class DishRatingCalculator
{
    public static double? GetAverageRating(this Dish dish)
    {
        ArgumentNullException.ThrowIfNull(dish);

        return new[] { dish.RatingA, dish.RatingR }
            .Where(rating => rating != (int)DishRating.None)
            .Select(rating => (double?)rating)
            .Average();
    }
}