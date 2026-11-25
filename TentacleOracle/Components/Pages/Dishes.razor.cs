using Microsoft.AspNetCore.Components;
using MudBlazor;
using TentacleOracle.BusinessLogic;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Enums;
using TentacleOracle.Extensions;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Dishes : BasePageWithControlsComponent<DishesControls>
{
    private bool _dishesLoaded;
    private bool _mealPlanBusy;
    private int? _selectedMealPlanEntryId;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

    [Inject] private MealPlanService MealPlan { get; set; } = null!;

    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Inject] private ILogger<Dishes> Logger { get; set; } = null!;

    private bool CanRemoveMeal =>
        !_mealPlanBusy && MealPlan.Entries.Any(entry => entry.Id == _selectedMealPlanEntryId);

    private bool CanAddMeal => !_mealPlanBusy && DishStateService.SelectedDish is { } selected
                                              && MealPlan.ContainsDish(selected.Id);

    private bool CanClearMealPlan => !_mealPlanBusy && MealPlan.Entries.Count > 0;

    private string? SelectedGeneratedDishElementId => _selectedMealPlanEntryId is { } id
        ? $"generated-dish-{id}"
        : null;

    private async Task AddSelectedDishToMealPlan()
    {
        if (!CanAddMeal || DishStateService.SelectedDish is not { } selected) return;
        await ChangeMealPlanAsync(() => MealPlan.AddDishAsync(selected.Id),
            "The dish could not be added to the meal plan. Please try again.", selectAddedEntry: true);
    }

    private void RemoveSelectedMeal()
    {
        if (!CanRemoveMeal) return;
        MealPlan.RemoveEntry(_selectedMealPlanEntryId!.Value);
        _selectedMealPlanEntryId = null;
    }

    private void ClearMealPlan()
    {
        if (_mealPlanBusy) return;
        MealPlan.Clear();
        _selectedMealPlanEntryId = null;
    }

    private async Task GenerateMealPlan()
    {
        if (!_dishesLoaded) return;
        await ChangeMealPlanAsync(MealPlan.FillAsync,
            "The meal plan could not be generated. Please try again.");
    }

    private async Task ChangeMealPlanAsync(Func<Task<MealPlanChangeResult>> change, string errorMessage,
        bool selectAddedEntry = false)
    {
        if (_mealPlanBusy) return;
        _mealPlanBusy = true;
        StateHasChanged();
        try
        {
            var result = await change();
            if (result == MealPlanChangeResult.LimitReached)
                await ShowMealPlanLimitAsync();
            else if (result == MealPlanChangeResult.Success && selectAddedEntry)
                _selectedMealPlanEntryId = MealPlan.Entries[^1].Id;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not update the meal plan.");
            await DialogService.ShowMessageAsync(MessageBoxType.Error, errorMessage);
        }
        finally
        {
            _mealPlanBusy = false;
            StateHasChanged();
        }
    }

    private Task<bool?> ShowMealPlanLimitAsync() => DialogService.ShowMessageAsync(MessageBoxType.Info,
        $"No more dishes can be added. The meal plan limit is {MealPlan.MealCount} meals. Increase the number in Settings or remove dishes from the plan.");

    private void SelectGeneratedDish(MealPlanEntry entry)
    {
        _selectedMealPlanEntryId = entry.Id;
    }

    private async Task LoadDishes()
    {
        try
        {
            await MealPlan.LoadAsync();
            _dishesLoaded = true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load dishes.");
            await DialogService.ShowMessageAsync(MessageBoxType.Error,
                "Dishes could not be loaded. Please reload the page to try again.");
        }
    }

    private void OnDishAdded(Dish dish)
    {
        MealPlan.UpdateDish(dish);
        StateHasChanged();
    }

    private void OnDishUpdated(Dish dish)
    {
        MealPlan.UpdateDish(dish);
        StateHasChanged();
    }

    private void OnDishDeleted(int dishId)
    {
        MealPlan.RemoveDish(dishId);
        if (!CanRemoveMeal) _selectedMealPlanEntryId = null;
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
        GC.SuppressFinalize(this);
    }

    #endregion
}