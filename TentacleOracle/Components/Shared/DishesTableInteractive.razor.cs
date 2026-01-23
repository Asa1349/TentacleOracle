using Microsoft.AspNetCore.Components;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Shared;

public partial class DishesTableInteractive : ComponentBase
{
    [Parameter]
    public List<Dish>? Dishes { get; set; }
    
    
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
    }
    #endregion
}