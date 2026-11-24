using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TentacleOracle.BusinessLogic;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Dishes : BasePageWithControlsComponent<DishesControls>
{
    private List<Dish>? _dishes;
    private List<Dish>? _generatedDishes;
    private int? _selectedGeneratedDishId;

    private string? SelectedGeneratedDishElementId => _selectedGeneratedDishId is { } id
        ? $"generated-dish-{id}"
        : null;

    [Inject] protected IDishService DishService { get; set; } = null!;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

    [Inject] protected IMealPlanGenerator MealPlanGenerator { get; set; } = null!;

    private void GenerateMealPlan()
    {
        if (_dishes == null) return;
        _generatedDishes = MealPlanGenerator.Generate(_dishes).ToList();
        _selectedGeneratedDishId = null;
        StateHasChanged();
    }

    private void SelectGeneratedDish(Dish dish)
    {
        _selectedGeneratedDishId = dish.Id;
    }

    private void HandleMealPlanKeyDown(KeyboardEventArgs args)
    {
        if (_generatedDishes is not { Count: > 0 }) return;

        var index = _generatedDishes.FindIndex(dish => dish.Id == _selectedGeneratedDishId);
        var nextIndex = args.Key switch
        {
            "ArrowDown" => Math.Min(index + 1, _generatedDishes.Count - 1),
            "ArrowUp" => index < 0 ? _generatedDishes.Count - 1 : Math.Max(index - 1, 0),
            "Home" => 0,
            "End" => _generatedDishes.Count - 1,
            "Enter" or " " => Math.Max(index, 0),
            _ => -1
        };

        if (nextIndex >= 0) SelectGeneratedDish(_generatedDishes[nextIndex]);
    }

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
        var generatedIndex = _generatedDishes?.FindIndex(existing => existing.Id == dish.Id) ?? -1;
        if (generatedIndex >= 0) _generatedDishes![generatedIndex] = dish;
        StateHasChanged();
    }

    private void OnDishDeleted(int dishId)
    {
        _dishes?.RemoveAll(dish => dish.Id == dishId);
        _generatedDishes?.RemoveAll(dish => dish.Id == dishId);
        if (_selectedGeneratedDishId == dishId) _selectedGeneratedDishId = null;
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
        DishStateService.MealPlanGenerationRequested += GenerateMealPlan;
        await LoadDishes();
    }

    public override void Dispose()
    {
        DishStateService.OnChange -= StateHasChanged;
        DishStateService.DishAdded -= OnDishAdded;
        DishStateService.DishUpdated -= OnDishUpdated;
        DishStateService.DishDeleted -= OnDishDeleted;
        DishStateService.MealPlanGenerationRequested -= GenerateMealPlan;
        base.Dispose();
    }

    #endregion
}