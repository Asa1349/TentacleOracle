using System.ComponentModel.DataAnnotations;

namespace TentacleOracle.Models;

public class AppSettings
{
    public int Id { get; set; } = 1;

    [Range(1, 28)] public int MealCount { get; set; } = 7;
}