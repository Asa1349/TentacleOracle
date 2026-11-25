using System.ComponentModel.DataAnnotations;

namespace TentacleOracle.Models;

public class Dish
{
    public int Id { get; init; }

    [MaxLength(100)] public string Name { get; init; } = string.Empty;

    [MaxLength(100)] public string Category { get; init; } = string.Empty;

    public int Kcal { get; init; }
    public int Effort { get; init; }
    public int RatingA { get; init; }
    public int RatingR { get; init; }
}