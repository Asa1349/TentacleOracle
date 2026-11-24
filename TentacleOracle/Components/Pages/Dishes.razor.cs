using Microsoft.AspNetCore.Components;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Dishes : BasePageWithControlsComponent<DishesControls>
{
    [Inject]
    protected IDishService DishService { get; set; } = null!;
    [Inject]
    protected DishStateService DishStateService { get; set; } = null!;
    
    private List<Dish>? _dishes;
    
    private async Task LoadDishes()
    {
        _dishes = await DishService.GetDishesAsync();
    }

    private void OnDishAdded(Dish dish)
    {
        _dishes ??= new List<Dish>();
        _dishes.Add(dish);
        StateHasChanged();
    }

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        DishStateService.OnChange += StateHasChanged;
        DishStateService.DishAdded += OnDishAdded;
        await LoadDishes();
    }
    
    public override void Dispose()
    {
        DishStateService.OnChange -= StateHasChanged;
        DishStateService.DishAdded -= OnDishAdded;
        base.Dispose();
    }
    
    #endregion
    
}