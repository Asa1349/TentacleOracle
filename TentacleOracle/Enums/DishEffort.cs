using System.ComponentModel.DataAnnotations;

namespace TentacleOracle.Enums;

public enum DishEffort
{
    [Display(Name = "None")] None = 0,
    [Display(Name = "1 - Little Effort")] Little = 1,
    [Display(Name = "2 - Some Effort")] Some = 2,
    [Display(Name = "3 - Medium Effort")] Medium = 3,
    [Display(Name = "4 - High Effort")] High = 4,

    [Display(Name = "5 - Very High Effort")]
    VeryHigh = 5
}