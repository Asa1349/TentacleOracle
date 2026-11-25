using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.Services;

[TestFixture]
public sealed class AsyncStateServiceTests
{
    [TestCaseSource(typeof(AsyncEventCases), nameof(AsyncEventCases.All))]
    public async Task Request_WithoutSubscribers_Completes(AsyncEventKind kind)
    {
        var handlers = AsyncEventCases.Create(kind);

        await handlers.Request();
    }

    [TestCaseSource(typeof(AsyncEventCases), nameof(AsyncEventCases.All))]
    public async Task Request_AwaitsEveryHandlerInSubscriptionOrder(AsyncEventKind kind)
    {
        var handlers = AsyncEventCases.Create(kind);
        var order = new List<string>();
        var firstMayComplete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        handlers.Subscribe(async () =>
        {
            order.Add("first-start");
            await firstMayComplete.Task;
            order.Add("first-end");
        });
        handlers.Subscribe(() => { order.Add("second"); return Task.CompletedTask; });

        var pending = handlers.Request();
        var wasPending = !pending.IsCompleted;
        var beforeCompletion = order.ToArray();
        firstMayComplete.SetResult();
        await pending.WaitAsync(TimeSpan.FromSeconds(5));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(wasPending, Is.True);
            Assert.That(beforeCompletion, Is.EqualTo(new[] { "first-start" }));
            Assert.That(order, Is.EqualTo(new[] { "first-start", "first-end", "second" }));
        }
    }

    [TestCaseSource(typeof(AsyncEventCases), nameof(AsyncEventCases.All))]
    public async Task Request_DoesNotCallUnsubscribedHandlers(AsyncEventKind kind)
    {
        var handlers = AsyncEventCases.Create(kind);
        var calls = 0;
        Func<Task> handler = () => { calls++; return Task.CompletedTask; };
        handlers.Subscribe(handler);
        handlers.Unsubscribe(handler);

        await handlers.Request();

        Assert.That(calls, Is.Zero);
    }

    [TestCaseSource(typeof(AsyncEventCases), nameof(AsyncEventCases.All))]
    public void Request_AsynchronousHandlerFailure_PropagatesAndStopsLaterHandlers(AsyncEventKind kind)
    {
        var handlers = AsyncEventCases.Create(kind);
        var failure = new InvalidOperationException("Test handler failure");
        var laterHandlerCalled = false;
        handlers.Subscribe(async () => { await Task.Yield(); throw failure; });
        handlers.Subscribe(() => { laterHandlerCalled = true; return Task.CompletedTask; });

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await handlers.Request());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception, Is.SameAs(failure));
            Assert.That(laterHandlerCalled, Is.False);
        }
    }

    [TestCaseSource(typeof(AsyncEventCases), nameof(AsyncEventCases.All))]
    public void Request_SynchronousHandlerFailure_Propagates(AsyncEventKind kind)
    {
        var handlers = AsyncEventCases.Create(kind);
        var failure = new InvalidOperationException("Test handler failure");
        handlers.Subscribe(() => throw failure);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await handlers.Request());

        Assert.That(exception, Is.SameAs(failure));
    }
}
