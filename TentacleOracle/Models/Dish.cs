using System.ComponentModel.DataAnnotations;

namespace TentacleOracle.Models;

public class Dish
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    public int Kcal { get; set; }
    public int Effort { get; set; }
    public int RatingA { get; set; }
    public int RatingR { get; set; }
}