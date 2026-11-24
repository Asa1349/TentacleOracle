using Microsoft.AspNetCore.Components;
using MudBlazor;
using TentacleOracle.Components.Dialog;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Controls;

public partial class DishesControls : ComponentBase, IDisposable
{
    [Inject]
    protected IDishService DishService { get; set; } = null!;
    [Inject]
    protected DishStateService DishStateService { get; set; } = null!;
    [Inject]
    protected NavigationManager Navigation { get; set; } = null!;
    [Inject]
    protected IDialogService DialogService { get; set; } = null!;

    private Dish? SelectedDish => DishStateService.SelectedDish;
    private bool _addDialogOpen;

   private void GenerateWeeklyPlan()
    {
        // TODO: Implement
    }
    
    private async Task AddDish()
    {
        if (_addDialogOpen) return;
        _addDialogOpen = true;

        // TODO: Auslagern
        try
        {
            var dishes = await DishService.GetDishesAsync();
            var categories = dishes.Select(dish => dish.Category.Trim())
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(category => category)
                .ToList();

            var parameters = new DialogParameters<AddDishDialog>
            {
                { dialog => dialog.Categories, categories }
            };
            var options = new DialogOptions
            {
                NoHeader = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium,
                BackdropClick = false,
                CloseOnEscapeKey = false
            };

            var dialog = await DialogService.ShowAsync<AddDishDialog>("Add Dish", parameters, options);
            var result = await dialog.Result;
            if (result is { Canceled: false, Data: Dish addedDish })
            {
                DishStateService.NotifyDishAdded(addedDish);
            }
        }
        finally
        {
            _addDialogOpen = false;
        }
    }

    private void EditSelectedDish()
    {
        if (SelectedDish == null) return;
        Navigation.NavigateTo($"/dishes/edit/{SelectedDish.Id}");
    }

    private async Task DeleteSelectedDish()
    {
        if (SelectedDish == null) return;
        
        // TODO: Confirmation Dialog
        
        await DishService.DeleteDishAsync(SelectedDish.Id);
        DishStateService.SelectedDish = null;
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