using Moq;
using TentacleOracle.Enums;
using TentacleOracle.Models;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.BusinessLogic;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class MealPlanServiceAsyncTests
{
    private readonly MealPlanTestContext _context = new();

    [TestCaseSource(typeof(MealPlanCases), nameof(MealPlanCases.Operations))]
    public async Task Operation_ForwardsCancellationToken(MealPlanOperation operation)
    {
        await _context.Plan.LoadAsync();
        using var lifetime = new CancellationTokenSource();

        await _context.RunAsync(operation, lifetime.Token);

        if (operation == MealPlanOperation.Load)
            _context.DishService.Verify(service => service.GetDishesAsync(lifetime.Token), Times.Once);
        else
            _context.SettingsService.Verify(service => service.GetMealCountAsync(lifetime.Token), Times.Once);
    }

    [TestCaseSource(typeof(MealPlanCases), nameof(MealPlanCases.Operations))]
    public async Task Operation_AlreadyCanceled_LeavesStateUnchanged(MealPlanOperation operation)
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        var catalog = _context.Plan.Dishes.ToArray();
        var entries = _context.Plan.Entries.ToArray();
        var limit = _context.Plan.MealCount;
        using var lifetime = new CancellationTokenSource();
        lifetime.Cancel();

        Assert.CatchAsync<OperationCanceledException>(() => _context.RunAsync(operation, lifetime.Token));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.Dishes, Is.EqualTo(catalog));
            Assert.That(_context.Plan.Entries, Is.EqualTo(entries));
            Assert.That(_context.Plan.MealCount, Is.EqualTo(limit));
            _context.Generator.Verify(generator => generator.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()),
                Times.Never);
        }
    }

    [TestCaseSource(typeof(MealPlanCases), nameof(MealPlanCases.Operations))]
    public async Task Operation_LateResponseAfterCancellation_CannotChangeState(MealPlanOperation operation)
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        var catalog = _context.Plan.Dishes.ToArray();
        var entries = _context.Plan.Entries.ToArray();
        var limit = _context.Plan.MealCount;
        var response = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _context.DishService.Setup(service => service.GetDishesAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken _) =>
            {
                await response.Task;
                return [DishFactory.Create(99)];
            });
        _context.SettingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken _) =>
            {
                await response.Task;
                return 3;
            });
        using var lifetime = new CancellationTokenSource();

        var pending = _context.RunAsync(operation, lifetime.Token);
        lifetime.Cancel();
        response.SetResult();

        Assert.CatchAsync<OperationCanceledException>(async () => await pending);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.Dishes, Is.EqualTo(catalog));
            Assert.That(_context.Plan.Entries, Is.EqualTo(entries));
            Assert.That(_context.Plan.MealCount, Is.EqualTo(limit));
        }
    }

    [TestCaseSource(typeof(MealPlanCases), nameof(MealPlanCases.Operations))]
    public async Task Operation_LoadFailure_PropagatesWithoutChangingEntries(MealPlanOperation operation)
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        var entries = _context.Plan.Entries.ToArray();
        var catalog = _context.Plan.Dishes.ToArray();
        var failure = new InvalidOperationException("Test database failure");
        _context.DishService.Setup(service => service.GetDishesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);
        _context.SettingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _context.RunAsync(operation));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception, Is.SameAs(failure));
            Assert.That(_context.Plan.Entries, Is.EqualTo(entries));
            Assert.That(_context.Plan.Dishes, Is.EqualTo(catalog));
        }
    }

    [Test]
    public async Task FillAsync_CancellationDuringGeneration_DoesNotAppendResults()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        var existing = _context.Plan.Entries.ToArray();
        using var lifetime = new CancellationTokenSource();
        _context.Generator.Setup(generator => generator.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()))
            .Returns((IReadOnlyList<Dish> dishes, int count) =>
            {
                lifetime.Cancel();
                return dishes.Take(count).ToArray();
            });

        Assert.CatchAsync<OperationCanceledException>(async () => await _context.Plan.FillAsync(lifetime.Token));

        Assert.That(_context.Plan.Entries, Is.EqualTo(existing));
    }

    [Test]
    public async Task FillAsync_GeneratorFailure_PreservesEntriesAndAllowsLaterOperation()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        var existing = _context.Plan.Entries.ToArray();
        _context.Generator.Setup(generator => generator.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()))
            .Throws(new InvalidOperationException("Test generator failure"));

        Assert.ThrowsAsync<InvalidOperationException>(async () => await _context.Plan.FillAsync());
        Assert.That(_context.Plan.Entries, Is.EqualTo(existing));
        _context.Generator.Setup(generator => generator.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()))
            .Returns((IReadOnlyList<Dish> dishes, int count) => dishes.Take(count).ToArray());

        Assert.That(await _context.Plan.FillAsync(), Is.EqualTo(MealPlanChangeResult.Success));
    }
}