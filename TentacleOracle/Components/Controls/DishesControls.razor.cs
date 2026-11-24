using Microsoft.AspNetCore.Components;
using MudBlazor;
using TentacleOracle.Components.Dialog;
using TentacleOracle.Extensions;
using TentacleOracle.Models;
using TentacleOracle.Models.Enums;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Controls;

public partial class DishesControls : ComponentBase, IDisposable
{
    private bool _dishDialogOpen;

    [Inject] protected IDishService DishService { get; set; } = null!;

    [Inject] protected DishStateService DishStateService { get; set; } = null!;

    [Inject] protected IDialogService DialogService { get; set; } = null!;

    [Inject] private ILogger<DishesControls> Logger { get; set; } = null!;

    private Dish? SelectedDish => DishStateService.SelectedDish;

    private void GenerateWeeklyPlan()
    {
        // TODO: Implement
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
        if (_dishDialogOpen) return;
        _dishDialogOpen = true;

        try
        {
            var dishes = await DishService.GetDishesAsync();
            var categories = dishes.Select(dish => dish.Category.Trim())
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(category => category)
                .ToList();

            var parameters = new DialogParameters<DishDialog>
            {
                { dialog => dialog.Categories, categories },
                { dialog => dialog.DishToEdit, dishToEdit }
            };
            var options = new DialogOptions
            {
                NoHeader = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium,
                BackdropClick = false,
                CloseOnEscapeKey = false
            };

            var title = dishToEdit == null ? "Add Dish" : "Edit Dish";
            var dialog = await DialogService.ShowAsync<DishDialog>(title, parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false, Data: Dish savedDish })
            {
                if (dishToEdit == null)
                    DishStateService.NotifyDishAdded(savedDish);
                else
                    DishStateService.NotifyDishUpdated(savedDish);
            }
        }
        finally
        {
            _dishDialogOpen = false;
        }
    }

    private async Task DeleteSelectedDish()
    {
        if (_dishDialogOpen || SelectedDish is not { } dish) return;
        _dishDialogOpen = true;

        try
        {
            var message = $"Are you sure you want to remove the dish '{dish.Name}'?";
            var confirmed = await DialogService.ShowMessageAsync(MessageBoxType.Warning,
                message, MessageBoxButtons.YesNo);
            if (confirmed != true) return;

            try
            {
                await DishService.DeleteDishAsync(dish.Id);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Could not delete dish {DishId}.", dish.Id);
                var errorMessage = $"The dish '{dish.Name}' could not be deleted. Please try again.";
                await DialogService.ShowMessageAsync(MessageBoxType.Error, errorMessage);
                return;
            }

            DishStateService.NotifyDishDeleted(dish.Id);
        }
        finally
        {
            _dishDialogOpen = false;
        }
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