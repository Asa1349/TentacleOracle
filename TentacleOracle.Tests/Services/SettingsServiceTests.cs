using Microsoft.EntityFrameworkCore;
using TentacleOracle.Models;
using TentacleOracle.Services;
using TentacleOracle.Tests.TestData;

namespace TentacleOracle.Tests.Services;

[TestFixture]
public sealed class SettingsServiceTests
{
    [Test]
    public async Task GetMealCountAsync_ReturnsPersistedCount()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = await database.ContextFactory.CreateDbContextAsync())
        {
            var settings = await context.AppSettings.SingleAsync(item => item.Id == 1);
            settings.MealCount = 12;
            await context.SaveChangesAsync();
        }

        var result = await new SettingsService(database.ContextFactory).GetMealCountAsync();

        Assert.That(result, Is.EqualTo(12));
    }

    [Test]
    public async Task GetMealCountAsync_WhenNoSettingsExist_ReturnsDefault()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = await database.ContextFactory.CreateDbContextAsync())
        {
            context.AppSettings.RemoveRange(context.AppSettings);
            await context.SaveChangesAsync();
        }

        var result = await new SettingsService(database.ContextFactory).GetMealCountAsync();

        Assert.That(result, Is.EqualTo(AppSettings.DefaultMealCount));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SaveMealCountAsync_InsertsOrUpdatesSettings(bool rowExists)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        if (!rowExists)
        {
            await using var context = await database.ContextFactory.CreateDbContextAsync();
            context.AppSettings.RemoveRange(context.AppSettings);
            await context.SaveChangesAsync();
        }

        var service = new SettingsService(database.ContextFactory);

        await service.SaveMealCountAsync(14);

        await using var verification = await database.ContextFactory.CreateDbContextAsync();
        var settings = await verification.AppSettings.SingleAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.Id, Is.EqualTo(1));
            Assert.That(settings.MealCount, Is.EqualTo(14));
        }
    }

    [TestCase(AppSettings.MinimumMealCount - 1)]
    [TestCase(AppSettings.MaximumMealCount + 1)]
    public async Task SaveMealCountAsync_OutOfRange_ThrowsWithoutDatabaseWrite(int count)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var service = new SettingsService(database.ContextFactory);

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveMealCountAsync(count));

        await using var verification = await database.ContextFactory.CreateDbContextAsync();
        var settings = await verification.AppSettings.SingleAsync(item => item.Id == 1);
        Assert.That(settings.MealCount, Is.EqualTo(AppSettings.DefaultMealCount));
    }
}