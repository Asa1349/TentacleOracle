using Microsoft.AspNetCore.Components;
using TentacleOracle.Models;

namespace TentacleOracle.Components.Shared;

public partial class DishesTableInteractive : ComponentBase
{
    [Parameter] public IReadOnlyList<Dish>? Dishes { get; set; }


    private void SelectDish(Dish dish)
    {
        DishStateService.SelectedDish = dish;
    }

    #region Overrides

    protected override void OnInitialized()
    {
        DishStateService.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        DishStateService.OnChange -= StateHasChanged;
        GC.SuppressFinalize(this);
    }

    #endregion
}