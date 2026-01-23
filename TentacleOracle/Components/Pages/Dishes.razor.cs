using FoodOracle.Models;
using Microsoft.AspNetCore.Components;
using TentacleOracle.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Dishes : BasePageWithControlsComponent<DishesControls>
{
    [Inject]
    protected IDishService DishService { get; set; } = default!;
    [Inject]
    protected DishStateService DishStateService { get; set; } = default!;
    
    private List<Dish>? _dishes;
    
    private async Task LoadDishes()
    {
        _dishes = await DishService.GetDishesAsync();
    }

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        DishStateService.OnChange += StateHasChanged;
        await LoadDishes();
    }
    
    public override void Dispose()
    {
        DishStateService.OnChange -= StateHasChanged;
        base.Dispose();
    }
    
    #endregion
    
}