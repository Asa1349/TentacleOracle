using TentacleOracle.Services;

namespace TentacleOracle.Tests.TestData;

public enum AsyncEventKind
{
    Save,
    Reset,
    GenerateMealPlan
}

public static class AsyncEventCases
{
    public static IEnumerable<AsyncEventKind> All => Enum.GetValues<AsyncEventKind>();

    public static (Action<Func<Task>> Subscribe, Action<Func<Task>> Unsubscribe, Func<Task> Request)
        Create(AsyncEventKind kind)
    {
        var settings = new SettingsStateService();
        var dishes = new DishStateService();
        return kind switch
        {
            AsyncEventKind.Save =>
                (handler => settings.SaveRequested += handler, handler => settings.SaveRequested -= handler,
                    settings.RequestSaveAsync),
            AsyncEventKind.Reset =>
                (handler => settings.ResetRequested += handler, handler => settings.ResetRequested -= handler,
                    settings.RequestResetAsync),
            AsyncEventKind.GenerateMealPlan =>
                (handler => dishes.MealPlanGenerationRequested += handler,
                    handler => dishes.MealPlanGenerationRequested -= handler, dishes.RequestMealPlanGenerationAsync),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}
