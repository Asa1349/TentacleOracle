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
    private List<MealPlanEntry>? _generatedDishes;
    private int _nextMealPlanEntryId;
    private int? _selectedMealPlanEntryId;
    [Inject] protected IDishService DishService { get; set; } = null!;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

    [Inject] protected IMealPlanGenerator MealPlanGenerator { get; set; } = null!;

    [Inject] private SettingsStateService SettingsState { get; set; } = null!;

    private bool CanRemoveMeal => _generatedDishes?.Any(entry => entry.Id == _selectedMealPlanEntryId) == true;

    private bool CanAddMeal => DishStateService.SelectedDish is { } selected
                               && _dishes?.Any(dish => dish.Id == selected.Id) == true;

    private bool CanClearMealPlan => _generatedDishes is { Count: > 0 };

    private string? SelectedGeneratedDishElementId => _selectedMealPlanEntryId is { } id
        ? $"generated-dish-{id}"
        : null;

    private void AddSelectedDishToMealPlan()
    {
        if (!CanAddMeal || DishStateService.SelectedDish is not { } selected) return;
        var dish = _dishes!.First(dish => dish.Id == selected.Id);
        _generatedDishes ??= [];
        var entry = new MealPlanEntry(++_nextMealPlanEntryId, dish);
        _generatedDishes.Add(entry);
        _selectedMealPlanEntryId = entry.Id;
    }

    private void RemoveSelectedMeal()
    {
        if (!CanRemoveMeal) return;
        _generatedDishes!.RemoveAll(entry => entry.Id == _selectedMealPlanEntryId);
        _selectedMealPlanEntryId = null;
    }

    private void ClearMealPlan()
    {
        _generatedDishes = [];
        _selectedMealPlanEntryId = null;
    }


    private void GenerateMealPlan()
    {
        if (_dishes == null) return;
        _generatedDishes = MealPlanGenerator.Generate(_dishes, SettingsState.MealCount)
            .Select(dish => new MealPlanEntry(++_nextMealPlanEntryId, dish)).ToList();
        _selectedMealPlanEntryId = null;
        StateHasChanged();
    }

    private void SelectGeneratedDish(MealPlanEntry entry)
    {
        _selectedMealPlanEntryId = entry.Id;
    }

    private void HandleMealPlanKeyDown(KeyboardEventArgs args)
    {
        if (_generatedDishes is not { Count: > 0 }) return;

        var index = _generatedDishes.FindIndex(entry => entry.Id == _selectedMealPlanEntryId);
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
        if (_generatedDishes != null)
            foreach (var entry in _generatedDishes.Where(entry => entry.Dish.Id == dish.Id))
                entry.Dish = dish;
        StateHasChanged();
    }

    private void OnDishDeleted(int dishId)
    {
        _dishes?.RemoveAll(dish => dish.Id == dishId);
        _generatedDishes?.RemoveAll(entry => entry.Dish.Id == dishId);
        if (!CanRemoveMeal) _selectedMealPlanEntryId = null;
        StateHasChanged();
    }

    private sealed class MealPlanEntry(int id, Dish dish)
    {
        public int Id { get; } = id;
        public Dish Dish { get; set; } = dish;
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