using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;
using TentacleOracle.Models;

namespace TentacleOracle.Services;

public sealed class SettingsService(IDbContextFactory<AppDbContext> contextFactory) : ISettingsService
{
    public async Task<int> GetMealCountAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var settings = await db.AppSettings.AsNoTracking().SingleOrDefaultAsync(settings => settings.Id == 1);
        return settings?.MealCount ?? AppSettings.DefaultMealCount;
    }

    public async Task SaveMealCountAsync(int count)
    {
        if (count is < AppSettings.MinimumMealCount or > AppSettings.MaximumMealCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        await using var db = await contextFactory.CreateDbContextAsync();
        var settings = await db.AppSettings.FindAsync(1);
        if (settings == null)
        {
            settings = new AppSettings { Id = 1 };
            db.AppSettings.Add(settings);
        }

        settings.MealCount = count;
        await db.SaveChangesAsync();
    }
}