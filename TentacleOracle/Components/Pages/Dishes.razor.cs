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
    private string? _loadError;
    private bool _mealPlanBusy;
    private string? _mealPlanError;
    private int? _selectedMealPlanEntryId;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

    [Inject] private MealPlanService MealPlan { get; set; } = null!;

    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Inject] private ILogger<Dishes> Logger { get; set; } = null!;

    private bool CanRemoveMeal =>
        !IsDisposed && !_mealPlanBusy && MealPlan.Entries.Any(entry => entry.Id == _selectedMealPlanEntryId);

    private bool CanAddMeal => !IsDisposed && _dishesLoaded && !_mealPlanBusy
                               && DishStateService.SelectedDish is { } selected
                               && MealPlan.ContainsDish(selected.Id);

    private bool CanClearMealPlan => !IsDisposed && !_mealPlanBusy && MealPlan.Entries.Count > 0;

    private string? SelectedGeneratedDishElementId => _selectedMealPlanEntryId is { } id
        ? $"generated-dish-{id}"
        : null;

    private async Task AddSelectedDishToMealPlan()
    {
        if (!CanAddMeal || DishStateService.SelectedDish is not { } selected) return;
        await ChangeMealPlanAsync(token => MealPlan.AddDishAsync(selected.Id, token),
            "The dish could not be added to the meal plan. Please reload the page.", selectAddedEntry: true);
    }

    private void RemoveSelectedMeal()
    {
        if (!CanRemoveMeal) return;
        MealPlan.RemoveEntry(_selectedMealPlanEntryId!.Value);
        _selectedMealPlanEntryId = null;
    }

    private void ClearMealPlan()
    {
        if (IsDisposed || _mealPlanBusy) return;
        MealPlan.Clear();
        _selectedMealPlanEntryId = null;
    }

    private async Task GenerateMealPlan()
    {
        if (!_dishesLoaded) return;
        await ChangeMealPlanAsync(MealPlan.FillAsync,
            "The meal plan could not be generated. Please reload the page.");
    }

    private async Task ChangeMealPlanAsync(Func<CancellationToken, Task<MealPlanChangeResult>> change,
        string errorMessage,
        bool selectAddedEntry = false)
    {
        if (IsDisposed || _mealPlanBusy) return;
        _mealPlanBusy = true;
        _mealPlanError = null;
        Refresh();
        try
        {
            var result = await change(LifetimeToken);
            if (IsDisposed) return;
            switch (result)
            {
                case MealPlanChangeResult.LimitReached:
                    await ShowMealPlanLimitAsync();
                    break;
                case MealPlanChangeResult.Success:
                    if (selectAddedEntry)
                        _selectedMealPlanEntryId = MealPlan.Entries[^1].Id;
                    break;
                case MealPlanChangeResult.DishNotFound:
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected meal plan result: {result}.");
            }
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
            // Expected when leaving the page; no error feedback is needed.
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not update the meal plan.");
            _mealPlanError = errorMessage;
        }
        finally
        {
            if (!IsDisposed)
            {
                _mealPlanBusy = false;
                Refresh();
            }
        }
    }

    private Task<bool?> ShowMealPlanLimitAsync() => DialogService.ShowMessageAsync(MessageBoxType.Info,
        $"No more dishes can be added. The meal plan limit is {MealPlan.MealCount} meals. Increase the number in Settings or remove dishes from the plan.",
        cancellationToken: LifetimeToken);

    private void SelectGeneratedDish(MealPlanEntry entry)
    {
        if (IsDisposed) return;
        _selectedMealPlanEntryId = entry.Id;
    }

    private async Task LoadDishes()
    {
        if (IsDisposed) return;
        try
        {
            await MealPlan.LoadAsync(LifetimeToken);
            if (IsDisposed) return;
            _dishesLoaded = true;
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
            // Expected when leaving the page; no error feedback is needed.
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not load dishes.");
            _loadError = "Dishes could not be loaded. Please reload the page.";
        }
    }

    private void OnDishAdded(Dish dish)
    {
        if (IsDisposed) return;
        MealPlan.UpdateDish(dish);
        Refresh();
    }

    private void OnDishUpdated(Dish dish)
    {
        if (IsDisposed) return;
        MealPlan.UpdateDish(dish);
        Refresh();
    }

    private void OnDishDeleted(int dishId)
    {
        if (IsDisposed) return;
        MealPlan.RemoveDish(dishId);
        if (!CanRemoveMeal) _selectedMealPlanEntryId = null;
        Refresh();
    }

    #region Overrides

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (IsDisposed) return;
        DishStateService.OnChange += Refresh;
        DishStateService.DishAdded += OnDishAdded;
        DishStateService.DishUpdated += OnDishUpdated;
        DishStateService.DishDeleted += OnDishDeleted;
        DishStateService.MealPlanGenerationRequested += GenerateMealPlan;
        await LoadDishes();
    }

    public override void Dispose()
    {
        if (IsDisposed) return;
        DishStateService.OnChange -= Refresh;
        DishStateService.DishAdded -= OnDishAdded;
        DishStateService.DishUpdated -= OnDishUpdated;
        DishStateService.DishDeleted -= OnDishDeleted;
        DishStateService.MealPlanGenerationRequested -= GenerateMealPlan;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}