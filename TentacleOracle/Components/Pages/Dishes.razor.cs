using Microsoft.AspNetCore.Components;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Dishes : BasePageWithControlsComponent<DishesControls>
{
    private List<Dish>? _dishes;

    [Inject] protected IDishService DishService { get; set; } = null!;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

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

    private void OnDishUpdated(Dish dish)
    {
        var index = _dishes?.FindIndex(existing => existing.Id == dish.Id) ?? -1;
        if (index < 0) return;
        _dishes![index] = dish;
        StateHasChanged();
    }

    private void OnDishDeleted(int dishId)
    {
        _dishes?.RemoveAll(dish => dish.Id == dishId);
        StateHasChanged();
    }

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        DishStateService.OnChange += StateHasChanged;
        DishStateService.DishAdded += OnDishAdded;
        DishStateService.DishUpdated += OnDishUpdated;
        DishStateService.DishDeleted += OnDishDeleted;
        await LoadDishes();
    }

    public override void Dispose()
    {
        DishStateService.OnChange -= StateHasChanged;
        DishStateService.DishAdded -= OnDishAdded;
        DishStateService.DishUpdated -= OnDishUpdated;
        DishStateService.DishDeleted -= OnDishDeleted;
        base.Dispose();
    }

    #endregion
}