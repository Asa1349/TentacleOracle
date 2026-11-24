using TentacleOracle.Models;

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

    public event Action<Dish>? DishAdded;

    public void NotifyDishAdded(Dish dish)
    {
        DishAdded?.Invoke(dish);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}