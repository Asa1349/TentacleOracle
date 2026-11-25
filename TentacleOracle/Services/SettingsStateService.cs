namespace TentacleOracle.Services;

public sealed class SettingsStateService
{
    public const int MinimumMealCount = 1;
    public const int MaximumMealCount = 28;
    public const int DefaultMealCount = 7;

    public int MealCount { get; private set; } = DefaultMealCount;

    public event Action? SaveRequested;
    public event Action? ResetRequested;

    public void RequestSave()
    {
        SaveRequested?.Invoke();
    }

    public void RequestReset()
    {
        ResetRequested?.Invoke();
    }

    public void SaveMealCount(int count)
    {
        if (count < MinimumMealCount || count > MaximumMealCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        MealCount = count;
    }
}