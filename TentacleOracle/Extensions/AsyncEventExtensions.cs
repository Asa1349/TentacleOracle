namespace TentacleOracle.Extensions;

internal static class AsyncEventExtensions
{
    public static async Task InvokeSequentiallyAsync(this Func<Task>? handlers)
    {
        foreach (var handler in Delegate.EnumerateInvocationList(handlers))
            await handler();
    }
}