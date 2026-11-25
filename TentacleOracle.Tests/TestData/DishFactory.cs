using TentacleOracle.Models;

namespace TentacleOracle.Tests.TestData;

internal static class DishFactory
{
    public static Dish Create(int id = 1, string? name = null, int ratingA = 0, int ratingR = 0) => new()
    {
        Id = id,
        Name = name ?? $"Dish {id}",
        Category = "Test category",
        Kcal = 400,
        RatingA = ratingA,
        RatingR = ratingR
    };

    public static List<Dish> CreateMany(int count) => Enumerable.Range(1, count).Select(id => Create(id)).ToList();
}
