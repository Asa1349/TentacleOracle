using TentacleOracle.Extensions;

namespace TentacleOracle.Services;

public sealed class SettingsStateService
{
    public event Func<Task>? SaveRequested;
    public event Func<Task>? ResetRequested;

    public Task RequestSaveAsync() => SaveRequested.InvokeSequentiallyAsync();

    public Task RequestResetAsync() => ResetRequested.InvokeSequentiallyAsync();
}