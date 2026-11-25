using TentacleOracle.Enums;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.BusinessLogic;

public sealed class MealPlanService
{
    private readonly IDishService _dishService;
    private readonly List<Dish> _dishes = [];
    private readonly List<MealPlanEntry> _entries = [];
    private readonly IMealPlanGenerator _generator;
    private readonly ISettingsService _settingsService;
    private int _nextEntryId;

    public MealPlanService(IDishService dishService, ISettingsService settingsService, IMealPlanGenerator generator)
    {
        _dishService = dishService;
        _settingsService = settingsService;
        _generator = generator;
        Dishes = _dishes.AsReadOnly();
        Entries = _entries.AsReadOnly();
    }

    public IReadOnlyList<Dish> Dishes { get; }
    public IReadOnlyList<MealPlanEntry> Entries { get; }
    public int MealCount { get; private set; } = AppSettings.DefaultMealCount;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var dishes = await _dishService.GetDishesAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _dishes.Clear();
        _dishes.AddRange(dishes);
    }

    public bool ContainsDish(int dishId) => _dishes.Any(dish => dish.Id == dishId);

    public async Task<MealPlanChangeResult> AddDishAsync(int dishId, CancellationToken cancellationToken = default)
    {
        var mealCount = await _settingsService.GetMealCountAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        MealCount = mealCount;
        if (_entries.Count >= MealCount) return MealPlanChangeResult.LimitReached;

        var dish = _dishes.FirstOrDefault(dish => dish.Id == dishId);
        if (dish == null) return MealPlanChangeResult.DishNotFound;

        _entries.Add(new MealPlanEntry(++_nextEntryId, dish));
        return MealPlanChangeResult.Success;
    }

    public async Task<MealPlanChangeResult> FillAsync(CancellationToken cancellationToken = default)
    {
        var mealCount = await _settingsService.GetMealCountAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        MealCount = mealCount;
        var remaining = MealCount - _entries.Count;
        if (remaining <= 0) return MealPlanChangeResult.LimitReached;

        var existingDishIds = _entries.Select(entry => entry.Dish.Id).ToHashSet();
        var candidates = _dishes.Where(dish => !existingDishIds.Contains(dish.Id)).ToList();
        var additions = _generator.Generate(candidates, remaining);
        cancellationToken.ThrowIfCancellationRequested();
        _entries.AddRange(additions.Select(dish => new MealPlanEntry(++_nextEntryId, dish)));
        return MealPlanChangeResult.Success;
    }

    public void RemoveEntry(int entryId) => _entries.RemoveAll(entry => entry.Id == entryId);

    public void Clear() => _entries.Clear();

    public void UpdateDish(Dish dish)
    {
        var index = _dishes.FindIndex(existing => existing.Id == dish.Id);
        if (index < 0)
            _dishes.Add(dish);
        else
            _dishes[index] = dish;

        for (var entryIndex = 0; entryIndex < _entries.Count; entryIndex++)
        {
            var entry = _entries[entryIndex];
            if (entry.Dish.Id == dish.Id)
                _entries[entryIndex] = entry with { Dish = dish };
        }
    }

    public void RemoveDish(int dishId)
    {
        _dishes.RemoveAll(dish => dish.Id == dishId);
        _entries.RemoveAll(entry => entry.Dish.Id == dishId);
    }
}

public sealed record MealPlanEntry(int Id, Dish Dish);