using TentacleOracle.Models;
using TentacleOracle.Services;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.Services;

[TestFixture]
public sealed class DishStateServiceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void SelectedDish_NotifiesWithCurrentSelection(bool clearSelection)
    {
        var state = new DishStateService { SelectedDish = DishFactory.Create() };
        var selection = clearSelection ? null : DishFactory.Create(2);
        Dish? observed = null;
        var notifications = 0;
        state.OnChange += () => { observed = state.SelectedDish; notifications++; };

        state.SelectedDish = selection;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(state.SelectedDish, Is.SameAs(selection));
            Assert.That(observed, Is.SameAs(selection));
            Assert.That(notifications, Is.EqualTo(1));
        }
    }

    [Test]
    public void NotifyDishAdded_PublishesDishWithoutChangingSelection()
    {
        var selected = DishFactory.Create();
        var added = DishFactory.Create(2);
        var state = new DishStateService { SelectedDish = selected };
        Dish? received = null;
        state.DishAdded += dish => received = dish;

        state.NotifyDishAdded(added);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received, Is.SameAs(added));
            Assert.That(state.SelectedDish, Is.SameAs(selected));
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    public void NotifyDishUpdated_ReplacesSelectionOnlyForMatchingId(int updatedId)
    {
        var selected = DishFactory.Create(1);
        var updated = DishFactory.Create(updatedId, "Updated");
        var state = new DishStateService { SelectedDish = selected };
        Dish? received = null;
        Dish? selectionDuringEvent = null;
        var notifications = 0;
        state.DishUpdated += dish => { received = dish; selectionDuringEvent = state.SelectedDish; };
        state.OnChange += () => notifications++;

        state.NotifyDishUpdated(updated);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received, Is.SameAs(updated));
            Assert.That(state.SelectedDish, Is.SameAs(updatedId == 1 ? updated : selected));
            Assert.That(selectionDuringEvent, Is.SameAs(state.SelectedDish));
            Assert.That(notifications, Is.EqualTo(1));
        }
    }

    [TestCase(1)]
    [TestCase(2)]
    public void NotifyDishDeleted_ClearsSelectionOnlyForMatchingId(int deletedId)
    {
        var selected = DishFactory.Create(1);
        var state = new DishStateService { SelectedDish = selected };
        var receivedId = 0;
        Dish? selectionDuringEvent = null;
        var notifications = 0;
        state.DishDeleted += id => { receivedId = id; selectionDuringEvent = state.SelectedDish; };
        state.OnChange += () => notifications++;

        state.NotifyDishDeleted(deletedId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(receivedId, Is.EqualTo(deletedId));
            Assert.That(state.SelectedDish, Is.SameAs(deletedId == 1 ? null : selected));
            Assert.That(selectionDuringEvent, Is.SameAs(state.SelectedDish));
            Assert.That(notifications, Is.EqualTo(1));
        }
    }

    [Test]
    public void Notifications_WithoutSelectionOrSubscribers_DoNotThrow()
    {
        var state = new DishStateService();

        Assert.DoesNotThrow(() =>
        {
            state.NotifyDishAdded(DishFactory.Create());
            state.NotifyDishUpdated(DishFactory.Create());
            state.NotifyDishDeleted(1);
        });
    }
}
