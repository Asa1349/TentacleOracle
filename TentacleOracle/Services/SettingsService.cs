using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;
using TentacleOracle.Models;

namespace TentacleOracle.Services;

public sealed class SettingsService(IDbContextFactory<AppDbContext> contextFactory) : ISettingsService
{
    public async Task<int> GetMealCountAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await db.AppSettings.AsNoTracking()
            .SingleOrDefaultAsync(settings => settings.Id == 1, cancellationToken);
        return settings?.MealCount ?? AppSettings.DefaultMealCount;
    }

    public async Task SaveMealCountAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count is < AppSettings.MinimumMealCount or > AppSettings.MaximumMealCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await db.AppSettings.FindAsync([1], cancellationToken);
        if (settings == null)
        {
            settings = new AppSettings { Id = 1 };
            db.AppSettings.Add(settings);
        }

        settings.MealCount = count;
        await db.SaveChangesAsync(cancellationToken);
    }
}