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

    public event Action<Dish>? DishUpdated;

    public void NotifyDishAdded(Dish dish)
    {
        DishAdded?.Invoke(dish);
    }

    public void NotifyDishUpdated(Dish dish)
    {
        if (_selectedDish?.Id == dish.Id) _selectedDish = dish;

        DishUpdated?.Invoke(dish);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}