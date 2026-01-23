using FoodOracle.Models;
using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Controls;

public partial class DishesControls : ComponentBase, IDisposable
{
    [Inject]
    protected IDishService DishService { get; set; } = default!;
    [Inject]
    protected DishStateService DishStateService { get; set; } = default!;
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    private Dish? SelectedDish => DishStateService.SelectedDish;

   private void GenerateWeeklyPlan()
    {
        // TODO: Implement
    }
    
    private void AddDish()
    {
        Navigation.NavigateTo("/dishes/create");
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