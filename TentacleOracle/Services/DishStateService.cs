using FoodOracle.Models;

namespace TentacleOracle.Services;

public class DishStateService
{
    private Dish? _selectedDish;
    
    public Dish? SelectedDish
    {
        get => _selectedDish;
        set
        {
            _selectedDish = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}