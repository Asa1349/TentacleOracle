using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TentacleOracle.BusinessLogic;
using TentacleOracle.Components.Pages;
using TentacleOracle.Models;
using TentacleOracle.Services;
using TentacleOracle.Tests.TestData;
using TestContext = Bunit.TestContext;

namespace TentacleOracle.Tests.Components;

[TestFixture]
public sealed class DishesTests
{
    [Test]
    public async Task SelectingDishAndAddingIt_RendersItInGeneratedMealList()
    {
        using var context = new TestContext();
        var dish = DishFactory.Create(1, "Test dinner");
        var dishState = new DishStateService();
        var dishService = new Mock<IDishService>(MockBehavior.Strict);
        var settingsService = new Mock<ISettingsService>(MockBehavior.Strict);
        var generator = new Mock<IMealPlanGenerator>(MockBehavior.Strict);
        dishService.Setup(service => service.GetDishesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([dish]);
        settingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AppSettings.DefaultMealCount);
        generator.Setup(service => service.Generate(It.IsAny<IReadOnlyList<Dish>>(), It.IsAny<int>()))
            .Returns((IReadOnlyList<Dish> dishes, int count) => dishes.Take(count).ToArray());

        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        context.Services.AddSingleton(dishState);
        context.Services.AddSingleton(new NavigationStateService());
        context.Services.AddSingleton(new MealPlanService(dishService.Object, settingsService.Object,
            generator.Object));
        context.Services.AddSingleton(Mock.Of<IDialogService>());
        context.RenderComponent<MudPopoverProvider>();
        var cut = context.RenderComponent<Dishes>();

        await cut.Find("tbody tr").ClickAsync(new MouseEventArgs());
        await cut.Find("button[aria-label='Add to plan']").ClickAsync(new MouseEventArgs());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dishState.SelectedDish, Is.SameAs(dish));
            Assert.That(cut.Find(".generated-meal-list").TextContent, Does.Contain("Test dinner"));
            Assert.That(cut.Find("button[aria-label='Remove from plan']").HasAttribute("disabled"), Is.False);
        }
    }
}