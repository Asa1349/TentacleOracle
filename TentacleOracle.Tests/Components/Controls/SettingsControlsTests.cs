using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using TentacleOracle.Components.Controls;
using TentacleOracle.Services;
using TestContext = Bunit.TestContext;

namespace TentacleOracle.Tests.Components;

[TestFixture]
public sealed class SettingsControlsTests
{
    [Test]
    public async Task Buttons_RaiseTheCorrespondingSettingsRequests()
    {
        using var context = new TestContext();
        var state = new SettingsStateService();
        var saves = 0;
        var resets = 0;
        state.SaveRequested += () =>
        {
            saves++;
            return Task.CompletedTask;
        };
        state.ResetRequested += () =>
        {
            resets++;
            return Task.CompletedTask;
        };
        context.Services.AddSingleton(state);
        var cut = context.RenderComponent<SettingsControls>();

        await cut.FindAll("button")[0].ClickAsync(new MouseEventArgs());
        await cut.FindAll("button")[1].ClickAsync(new MouseEventArgs());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(saves, Is.EqualTo(1));
            Assert.That(resets, Is.EqualTo(1));
        }
    }
}