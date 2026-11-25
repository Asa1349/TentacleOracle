using Microsoft.EntityFrameworkCore;
using TentacleOracle.Models;
using TentacleOracle.Services;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.Services;

[TestFixture]
public sealed class DishServiceTests
{
    [Test]
    public async Task GetDishesAsync_ReturnsPersistedDishes()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = await database.ContextFactory.CreateDbContextAsync())
        {
            context.Dishes.AddRange(CreateDish("First"), CreateDish("Second"));
            await context.SaveChangesAsync();
        }

        var result = await new DishService(database.ContextFactory).GetDishesAsync();

        Assert.That(result.Select(dish => dish.Name), Is.EquivalentTo(new[] { "First", "Second" }));
    }

    [Test]
    public async Task AddDishAsync_PersistsDish()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var service = new DishService(database.ContextFactory);
        var dish = CreateDish("Added");

        await service.AddDishAsync(dish);

        await using var context = await database.ContextFactory.CreateDbContextAsync();
        Assert.That(await context.Dishes.Select(item => item.Name).SingleAsync(), Is.EqualTo("Added"));
    }

    [Test]
    public async Task UpdateDishAsync_UpdatesExistingDish()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var existing = CreateDish("Before");
        await using (var context = await database.ContextFactory.CreateDbContextAsync())
        {
            context.Dishes.Add(existing);
            await context.SaveChangesAsync();
        }

        var service = new DishService(database.ContextFactory);
        var updated = new Dish { Id = existing.Id, Name = "After", Category = "Updated", Kcal = 500 };

        await service.UpdateDishAsync(updated);

        await using var verification = await database.ContextFactory.CreateDbContextAsync();
        var persisted = await verification.Dishes.SingleAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(persisted.Name, Is.EqualTo("After"));
            Assert.That(persisted.Category, Is.EqualTo("Updated"));
            Assert.That(persisted.Kcal, Is.EqualTo(500));
        }
    }

    [Test]
    public async Task UpdateDishAsync_MissingDish_Throws()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var service = new DishService(database.ContextFactory);

        Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateDishAsync(CreateDish("Missing", 99)));
    }

    [Test]
    public async Task DeleteDishAsync_DeletesExistingDishAndIgnoresMissingDish()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var existing = CreateDish("To delete");
        await using (var context = await database.ContextFactory.CreateDbContextAsync())
        {
            context.Dishes.Add(existing);
            await context.SaveChangesAsync();
        }

        var service = new DishService(database.ContextFactory);

        await service.DeleteDishAsync(existing.Id);
        await service.DeleteDishAsync(int.MaxValue);

        await using var verification = await database.ContextFactory.CreateDbContextAsync();
        Assert.That(await verification.Dishes.CountAsync(), Is.Zero);
    }

    private static Dish CreateDish(string name, int id = 0) => new()
    {
        Id = id,
        Name = name,
        Category = "Test",
        Kcal = 400
    };
}