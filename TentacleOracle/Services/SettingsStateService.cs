namespace TentacleOracle.Services;

public sealed class SettingsStateService
{
    public event Func<Task>? SaveRequested;
    public event Func<Task>? ResetRequested;

    public async Task RequestSaveAsync()
    {
        foreach (var handler in Delegate.EnumerateInvocationList(SaveRequested))
            await handler();
    }

    public async Task RequestResetAsync()
    {
        foreach (var handler in Delegate.EnumerateInvocationList(ResetRequested))
            await handler();
    }
}