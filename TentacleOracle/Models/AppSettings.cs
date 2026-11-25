using System.ComponentModel.DataAnnotations;

namespace TentacleOracle.Models;

public class AppSettings
{
    public const int MinimumMealCount = 1;
    public const int MaximumMealCount = 28;
    public const int DefaultMealCount = 7;

    public int Id { get; init; } = 1;

    [Range(MinimumMealCount, MaximumMealCount)]
    public int MealCount { get; set; } = DefaultMealCount;
}