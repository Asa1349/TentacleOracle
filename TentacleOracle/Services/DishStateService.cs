using TentacleOracle.Extensions;
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

    public event Action<int>? DishDeleted;

    public event Func<Task>? MealPlanGenerationRequested;

    public Task RequestMealPlanGenerationAsync() => MealPlanGenerationRequested.InvokeSequentiallyAsync();

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

    public void NotifyDishDeleted(int dishId)
    {
        if (_selectedDish?.Id == dishId) _selectedDish = null;

        DishDeleted?.Invoke(dishId);
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}