using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using TentacleOracle.Components.Shared;
using TentacleOracle.Models;
using TentacleOracle.Services;
using TentacleOracle.Tests.TestData;
using TestContext = Bunit.TestContext;

namespace TentacleOracle.Tests.Components;

[TestFixture]
public sealed class DishesTableInteractiveTests
{
    [Test]
    public void Render_NullDishes_ShowsLoadingRow()
    {
        using var context = CreateContext();

        var cut = context.RenderComponent<DishesTableInteractive>();

        Assert.That(cut.Markup, Does.Contain("Loading..."));
    }

    [Test]
    public void Render_EmptyDishes_ShowsEmptyState()
    {
        using var context = CreateContext();

        var cut = context.RenderComponent<DishesTableInteractive>(parameters =>
            parameters.Add(component => component.Dishes, Array.Empty<Dish>()));

        Assert.That(cut.Markup, Does.Contain("No entries were found!"));
    }

    [Test]
    public async Task Render_Dishes_ShowsRatingsAndClickUpdatesSelection()
    {
        using var context = CreateContext();
        var state = context.Services.GetRequiredService<DishStateService>();
        var ratedDish = DishFactory.Create(1, "Rated dish", ratingA: 2, ratingR: 3);
        var unratedDish = DishFactory.Create(2, "Unrated dish");
        var cut = context.RenderComponent<DishesTableInteractive>(parameters =>
            parameters.Add(component => component.Dishes, new[] { ratedDish, unratedDish }));

        var rows = cut.FindAll("tbody tr");
        await rows[0].ClickAsync(new MouseEventArgs());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows[0].TextContent, Does.Contain("Rated dish"));
            Assert.That(rows[0].TextContent, Does.Contain(2.5.ToString(CultureInfo.CurrentCulture)));
            Assert.That(rows[1].TextContent, Does.Contain("Unrated dish"));
            Assert.That(rows[1].TextContent, Does.Contain("–"));
            Assert.That(state.SelectedDish, Is.SameAs(ratedDish));
            Assert.That(cut.Find("tr.selected").TextContent, Does.Contain("Rated dish"));
        }
    }

    private static TestContext CreateContext()
    {
        var context = new TestContext();
        context.Services.AddSingleton(new DishStateService());
        return context;
    }
}