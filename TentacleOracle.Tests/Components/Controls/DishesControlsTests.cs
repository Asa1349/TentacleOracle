using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using TentacleOracle.Components.Controls;
using TentacleOracle.Services;
using TentacleOracle.Tests.TestData;
using TestContext = Bunit.TestContext;

namespace TentacleOracle.Tests.Components;

[TestFixture]
public sealed class DishesControlsTests
{
    [Test]
    public async Task Generate_RaisesMealPlanGenerationRequest()
    {
        using var context = CreateContext(out var dishState, out _);
        var calls = 0;
        dishState.MealPlanGenerationRequested += () =>
        {
            calls++;
            return Task.CompletedTask;
        };
        var cut = context.RenderComponent<DishesControls>();

        await cut.FindAll("button")[0].ClickAsync(new MouseEventArgs());

        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public async Task GenerateFailure_ShowsErrorAndReEnablesControls()
    {
        using var context = CreateContext(out var dishState, out _);
        dishState.MealPlanGenerationRequested += () => throw new InvalidOperationException("Generation failed");
        var cut = context.RenderComponent<DishesControls>();

        await cut.FindAll("button")[0].ClickAsync(new MouseEventArgs());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cut.Find("[role='alert']").TextContent,
                Is.EqualTo("The meal plan could not be generated. Please reload the page."));
            Assert.That(cut.FindAll("button").Take(2).Any(button => button.HasAttribute("disabled")), Is.False);
        }
    }

    [Test]
    public async Task AddDish_LoadFailure_ShowsErrorAndRestoresButtons()
    {
        using var context = CreateContext(out _, out var dishService);
        dishService.Setup(service => service.GetDishesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var cut = context.RenderComponent<DishesControls>();

        await cut.FindAll("button")[1].ClickAsync(new MouseEventArgs());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cut.Find("[role='alert']").TextContent,
                Is.EqualTo("The dish dialog could not be loaded. Please reload the page."));
            Assert.That(cut.FindAll("button").Take(2).Any(button => button.HasAttribute("disabled")), Is.False);
        }
    }

    [Test]
    public async Task EditAndDelete_AreEnabledOnlyWhenDishIsSelected()
    {
        using var context = CreateContext(out var dishState, out _);
        var cut = context.RenderComponent<DishesControls>();
        var buttons = cut.FindAll("button");
        Assert.That(buttons[2].HasAttribute("disabled"), Is.True);
        Assert.That(buttons[3].HasAttribute("disabled"), Is.True);

        await cut.InvokeAsync(() => dishState.SelectedDish = DishFactory.Create());
        buttons = cut.FindAll("button");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(buttons[2].HasAttribute("disabled"), Is.False);
            Assert.That(buttons[3].HasAttribute("disabled"), Is.False);
        }
    }

    private static TestContext CreateContext(out DishStateService dishState,
        out Mock<IDishService> dishService)
    {
        var context = new TestContext();
        dishState = new DishStateService();
        dishService = new Mock<IDishService>(MockBehavior.Strict);
        context.Services.AddSingleton(dishState);
        context.Services.AddSingleton(dishService.Object);
        context.Services.AddSingleton(Mock.Of<IDialogService>());
        return context;
    }
}