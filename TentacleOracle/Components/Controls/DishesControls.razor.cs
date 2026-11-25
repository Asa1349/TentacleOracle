using Microsoft.AspNetCore.Components;
using MudBlazor;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Dialog;
using TentacleOracle.Enums;
using TentacleOracle.Extensions;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Controls;

public partial class DishesControls : LifetimeComponentBase
{
    private static readonly DialogOptions DishDialogOptions = new()
    {
        NoHeader = true,
        FullWidth = true,
        MaxWidth = MaxWidth.Medium,
        BackdropClick = false,
        CloseOnEscapeKey = false
    };

    private IDialogReference? _activeDialog;

    private bool _dishDialogOpen;
    private string? _error;
    private bool _generating;

    private bool IsBusy => IsDisposed || _dishDialogOpen || _generating;

    [Inject] protected IDishService DishService { get; set; } = null!;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

    [Inject] protected IDialogService DialogService { get; set; } = null!;

    [Inject] private ILogger<DishesControls> Logger { get; set; } = null!;

    private Dish? SelectedDish => DishStateService.SelectedDish;

    private async Task GenerateMealPlan()
    {
        if (IsBusy) return;
        _generating = true;
        _error = null;
        Refresh();
        try
        {
            await DishStateService.RequestMealPlanGenerationAsync().WaitAsync(LifetimeToken);
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
            // Expected when leaving the page; no error feedback is needed.
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not generate a meal plan.");
            _error = "The meal plan could not be generated. Please reload the page.";
        }
        finally
        {
            if (!IsDisposed)
            {
                _generating = false;
                Refresh();
            }
        }
    }

    private Task AddDish()
    {
        return ShowDishDialogAsync();
    }

    private async Task EditSelectedDish()
    {
        if (SelectedDish is { } dish) await ShowDishDialogAsync(dish);
    }

    private async Task ShowDishDialogAsync(Dish? dishToEdit = null)
    {
        if (IsBusy) return;
        _dishDialogOpen = true;
        _error = null;
        Refresh();

        try
        {
            await ShowDishEditorAsync(dishToEdit);
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
            // Expected when leaving the page; no error feedback is needed.
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not load or open the dish dialog.");
            _error = "The dish dialog could not be loaded. Please reload the page.";
        }
        finally
        {
            if (!IsDisposed)
            {
                _activeDialog = null;
                _dishDialogOpen = false;
                Refresh();
            }
        }
    }

    private async Task<List<string>> LoadCategoriesAsync()
    {
        var dishes = await DishService.GetDishesAsync(LifetimeToken);
        LifetimeToken.ThrowIfCancellationRequested();
        return dishes.Select(dish => dish.Category.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category)
            .ToList();
    }

    private async Task ShowDishEditorAsync(Dish? dishToEdit)
    {
        var categories = await LoadCategoriesAsync();
        if (IsDisposed) return;

        var parameters = new DialogParameters<DishDialog>
        {
            { dialog => dialog.Categories, categories },
            { dialog => dialog.DishToEdit, dishToEdit }
        };
        var title = dishToEdit == null ? "Add Dish" : "Edit Dish";
        var dialog = await DialogService.ShowAsync<DishDialog>(title, parameters, DishDialogOptions);
        if (IsDisposed)
        {
            dialog.Close();
            return;
        }

        _activeDialog = dialog;
        var result = await dialog.Result.WaitAsync(LifetimeToken);
        if (IsDisposed || result is not { Canceled: false, Data: Dish savedDish }) return;

        if (dishToEdit == null)
            DishStateService.NotifyDishAdded(savedDish);
        else
            DishStateService.NotifyDishUpdated(savedDish);
    }

    private async Task DeleteSelectedDish()
    {
        if (IsBusy || SelectedDish is not { } dish) return;
        _dishDialogOpen = true;
        _error = null;
        Refresh();

        try
        {
            var message = $"Are you sure you want to remove the dish '{dish.Name}'?";
            var confirmed = await DialogService.ShowMessageAsync(MessageBoxType.Warning,
                message, MessageBoxButtons.YesNo, LifetimeToken);
            if (IsDisposed || confirmed != true) return;

            await DishService.DeleteDishAsync(dish.Id, LifetimeToken);
            if (IsDisposed) return;
            DishStateService.NotifyDishDeleted(dish.Id);
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
            // Expected when leaving the page; no error feedback is needed.
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not delete dish {DishId}.", dish.Id);
            _error = $"The dish '{dish.Name}' could not be deleted. Please reload the page.";
        }
        finally
        {
            if (!IsDisposed)
            {
                _dishDialogOpen = false;
                Refresh();
            }
        }
    }


    #region Overrides

    protected override void OnInitialized()
    {
        DishStateService.OnChange += Refresh;
    }

    public override void Dispose()
    {
        if (IsDisposed) return;
        DishStateService.OnChange -= Refresh;
        base.Dispose();
        _activeDialog?.Close();
        GC.SuppressFinalize(this);
    }

    #endregion
}