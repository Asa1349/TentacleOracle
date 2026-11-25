using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Tests.Services;

[TestFixture]
public sealed class NavigationStateServiceTests
{
    [Test]
    public void SetControlType_NotifiesOnlyWhenTypeChanges()
    {
        var state = new NavigationStateService();
        var owner = new object();
        var notifications = 0;
        state.OnChange += () => notifications++;

        state.SetControlType<FirstControls>(owner);
        state.SetControlType<FirstControls>(owner);
        state.SetControlType<SecondControls>(owner);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(state.CurrentControlType, Is.EqualTo(typeof(SecondControls)));
            Assert.That(notifications, Is.EqualTo(2));
        }
    }

    [Test]
    public void ClearControls_PreviousOwnerCannotClearCurrentControls()
    {
        var state = new NavigationStateService();
        var previousOwner = new object();
        var currentOwner = new object();
        state.SetControlType<FirstControls>(previousOwner);
        state.SetControlType<FirstControls>(currentOwner);

        state.ClearControls(previousOwner);

        Assert.That(state.CurrentControlType, Is.EqualTo(typeof(FirstControls)));
        state.ClearControls(currentOwner);
        Assert.That(state.CurrentControlType, Is.Null);
    }

    [Test]
    public void ClearControls_CurrentOwner_NotifiesOnceAndIsIdempotent()
    {
        var state = new NavigationStateService();
        var owner = new object();
        state.SetControlType<FirstControls>(owner);
        var notifications = 0;
        state.OnChange += () => notifications++;

        state.ClearControls(owner);
        state.ClearControls(owner);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(state.CurrentControlType, Is.Null);
            Assert.That(notifications, Is.EqualTo(1));
        }
    }

    [Test]
    public void SetNoControls_RemovesControlsAndNotifies()
    {
        var state = new NavigationStateService();
        var owner = new object();
        state.SetControlType<FirstControls>(owner);
        var notifications = 0;
        state.OnChange += () => notifications++;

        state.SetNoControls(new object());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(state.CurrentControlType, Is.Null);
            Assert.That(notifications, Is.EqualTo(1));
        }
    }

    [TestCase(nameof(NavigationStateService.SetControlType))]
    [TestCase(nameof(NavigationStateService.SetNoControls))]
    [TestCase(nameof(NavigationStateService.ClearControls))]
    public void OwnerOperation_NullOwner_Throws(string operation)
    {
        var state = new NavigationStateService();
        Action action = operation switch
        {
            nameof(NavigationStateService.SetControlType) => () => state.SetControlType<FirstControls>(null!),
            nameof(NavigationStateService.SetNoControls) => () => state.SetNoControls(null!),
            nameof(NavigationStateService.ClearControls) => () => state.ClearControls(null!),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        Assert.Throws<ArgumentNullException>(() => action());
    }

    private sealed class FirstControls : ComponentBase { }
    private sealed class SecondControls : ComponentBase { }
}
