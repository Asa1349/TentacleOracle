using Moq;
using TentacleOracle.BusinessLogic;
using TentacleOracle.Enums;
using TentacleOracle.Models;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.BusinessLogic;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class MealPlanServiceTests
{
    private readonly MealPlanTestContext _context = new();

    [Test]
    public void NewPlan_StartsEmptyWithConfiguredDefault()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.Dishes, Is.Empty);
            Assert.That(_context.Plan.Entries, Is.Empty);
            Assert.That(_context.Plan.MealCount, Is.EqualTo(AppSettings.DefaultMealCount));
        }
    }

    [Test]
    public async Task LoadAsync_ReplacesCatalogWithoutChangingExistingPlan()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        var entry = _context.Plan.Entries.Single();
        _context.DishCatalog = [DishFactory.Create(10)];

        await _context.Plan.LoadAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.Dishes.Select(dish => dish.Id), Is.EqualTo(new[] { 10 }));
            Assert.That(_context.Plan.Entries.Single(), Is.SameAs(entry));
        }
    }

    [TestCase(1, true)]
    [TestCase(99, false)]
    public async Task ContainsDish_UsesCatalog(int dishId, bool expected)
    {
        await _context.Plan.LoadAsync();

        Assert.That(_context.Plan.ContainsDish(dishId), Is.EqualTo(expected));
    }

    [Test]
    public async Task AddDishAsync_UsesCatalogDishAndAllowsDuplicateOccurrences()
    {
        await _context.Plan.LoadAsync();

        var first = await _context.Plan.AddDishAsync(1);
        var second = await _context.Plan.AddDishAsync(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.EqualTo(MealPlanChangeResult.Success));
            Assert.That(second, Is.EqualTo(MealPlanChangeResult.Success));
            Assert.That(_context.Plan.Entries, Has.Count.EqualTo(2));
            Assert.That(_context.Plan.Entries.Select(entry => entry.Id), Is.Unique);
            Assert.That(_context.Plan.Entries.All(entry => ReferenceEquals(entry.Dish, _context.Plan.Dishes[0])),
                Is.True);
        }
    }

    [Test]
    public async Task AddDishAsync_UnknownDish_LeavesPlanUnchanged()
    {
        await _context.Plan.LoadAsync();

        var result = await _context.Plan.AddDishAsync(99);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(MealPlanChangeResult.DishNotFound));
            Assert.That(_context.Plan.Entries, Is.Empty);
        }
    }

    [TestCaseSource(typeof(MealPlanCases), nameof(MealPlanCases.Availability))]
    public async Task FillAsync_RespectsAvailability(int available, int requested, int expected)
    {
        _context.DishCatalog = DishFactory.CreateMany(available);
        _context.MealCount = requested;
        await _context.Plan.LoadAsync();

        var result = await _context.Plan.FillAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(MealPlanChangeResult.Success));
            Assert.That(_context.Plan.Entries, Has.Count.EqualTo(expected));
            Assert.That(_context.Plan.Entries.Select(entry => entry.Dish), Is.SubsetOf(_context.Plan.Dishes));
            Assert.That(_context.Plan.Entries.Select(entry => entry.Dish.Id), Is.Unique);
        }
    }

    [Test]
    public async Task FillAsync_PreservesEntriesAndExcludesExistingDishIds()
    {
        _context.MealCount = 4;
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(1);
        var existing = _context.Plan.Entries.ToArray();

        await _context.Plan.FillAsync();

        using (Assert.EnterMultipleScope())
        {
            _context.Generator.Verify(generator => generator.Generate(
                    It.Is<IReadOnlyList<Dish>>(dishes =>
                        dishes.Select(dish => dish.Id).SequenceEqual(new[] { 2, 3, 4 })), 2),
                Times.Once);
            Assert.That(_context.Plan.Entries.Take(2), Is.EqualTo(existing));
            Assert.That(_context.Plan.Entries.Skip(2).Select(entry => entry.Dish.Id), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(_context.Plan.Entries.Select(entry => entry.Id), Is.Unique);
        }
    }

    [TestCase(false, 2)]
    [TestCase(true, 2)]
    [TestCase(false, 1)]
    [TestCase(true, 1)]
    public async Task PlanChange_AtOrAboveCurrentLimit_IsRejected(bool fill, int limit)
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(2);
        var existing = _context.Plan.Entries.ToArray();
        _context.MealCount = limit;

        var result = fill ? await _context.Plan.FillAsync() : await _context.Plan.AddDishAsync(3);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(MealPlanChangeResult.LimitReached));
            Assert.That(_context.Plan.Entries, Is.EqualTo(existing));
            Assert.That(_context.Plan.MealCount, Is.EqualTo(limit));
            _context.Generator.Verify(generator => generator.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()),
                Times.Never);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task PlanChange_ReadsIncreasedLimit(bool fill)
    {
        _context.MealCount = 1;
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        _context.MealCount = 2;

        var result = fill ? await _context.Plan.FillAsync() : await _context.Plan.AddDishAsync(2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(MealPlanChangeResult.Success));
            Assert.That(_context.Plan.Entries, Has.Count.EqualTo(2));
            Assert.That(_context.Plan.MealCount, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task RemoveEntry_RemovesOnlyTheRequestedOccurrence()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(1);
        var remaining = _context.Plan.Entries[1];

        _context.Plan.RemoveEntry(_context.Plan.Entries[0].Id);
        _context.Plan.RemoveEntry(int.MaxValue);

        Assert.That(_context.Plan.Entries.Single(), Is.SameAs(remaining));
    }

    [Test]
    public async Task Clear_PreservesCatalogAndDoesNotReuseEntryIds()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.FillAsync();
        var previousId = _context.Plan.Entries.Max(entry => entry.Id);
        var catalog = _context.Plan.Dishes.ToArray();

        _context.Plan.Clear();

        Assert.That(_context.Plan.Entries, Is.Empty);
        await _context.Plan.AddDishAsync(1);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.Dishes, Is.EqualTo(catalog));
            Assert.That(_context.Plan.Entries.Single().Id, Is.GreaterThan(previousId));
        }
    }

    [Test]
    public async Task UpdateDish_ReplacesCatalogDishAndEveryPlannedOccurrence()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(2);
        var originalEntries = _context.Plan.Entries.ToArray();
        var updated = DishFactory.Create(1, "Updated dish");

        _context.Plan.UpdateDish(updated);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.Dishes.Single(dish => dish.Id == 1), Is.SameAs(updated));
            Assert.That(_context.Plan.Entries.Take(2).All(entry => ReferenceEquals(entry.Dish, updated)), Is.True);
            Assert.That(_context.Plan.Entries.Select(entry => entry.Id),
                Is.EqualTo(originalEntries.Select(entry => entry.Id)));
            Assert.That(_context.Plan.Entries[2], Is.SameAs(originalEntries[2]));
        }
    }

    [Test]
    public async Task UpdateDish_UnknownDish_AddsItToCatalog()
    {
        await _context.Plan.LoadAsync();
        var added = DishFactory.Create(99);

        _context.Plan.UpdateDish(added);

        Assert.That(_context.Plan.Dishes.Single(dish => dish.Id == 99), Is.SameAs(added));
    }

    [Test]
    public async Task RemoveDish_RemovesAllOccurrencesButPreservesOtherDishes()
    {
        await _context.Plan.LoadAsync();
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(1);
        await _context.Plan.AddDishAsync(2);

        _context.Plan.RemoveDish(1);
        _context.Plan.RemoveDish(99);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_context.Plan.ContainsDish(1), Is.False);
            Assert.That(_context.Plan.Dishes.Select(dish => dish.Id), Is.EquivalentTo(new[] { 2, 3, 4 }));
            Assert.That(_context.Plan.Entries.Single().Dish.Id, Is.EqualTo(2));
        }
    }

    [Test]
    public void Collections_CannotBeChangedByCallers()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Dish>)_context.Plan.Dishes).Add(DishFactory.Create()));
            Assert.Throws<NotSupportedException>(() => ((ICollection<MealPlanEntry>)_context.Plan.Entries).Clear());
        }
    }
}