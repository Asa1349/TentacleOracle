using System.ComponentModel.DataAnnotations;

namespace TentacleOracle.Models.Enums;

public enum DishRating
{
    [Display(Name = "None")]
    None = 0,
    [Display(Name = "1 - Very Tasty")]
    VeryTasty = 1,
    [Display(Name = "2 - Tasty")]
    Tasty = 2,
    [Display(Name = "3 - Okay")]
    Okay = 3,
    [Display(Name = "4 - So-so")]
    SoSo = 4,
    [Display(Name = "5 - Not Good")]
    NotGood = 5
}
