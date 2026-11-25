using Microsoft.EntityFrameworkCore;
using TentacleOracle.Data;
using TentacleOracle.Models;

namespace TentacleOracle.Services;

public sealed class SettingsStateService
{
    public const int MinimumMealCount = 1;
    public const int MaximumMealCount = 28;
    public const int DefaultMealCount = 7;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SettingsStateService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public int MealCount { get; private set; } = DefaultMealCount;

    public event Func<Task>? SaveRequested;
    public event Func<Task>? ResetRequested;

    public async Task RequestSaveAsync()
    {
        if (SaveRequested is { } handlers)
            foreach (Func<Task> handler in handlers.GetInvocationList())
                await handler();
    }

    public async Task RequestResetAsync()
    {
        if (ResetRequested is { } handlers)
            foreach (Func<Task> handler in handlers.GetInvocationList())
                await handler();
    }

    public async Task LoadAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var settings = await db.AppSettings.AsNoTracking().SingleOrDefaultAsync(settings => settings.Id == 1);
        MealCount = settings?.MealCount ?? DefaultMealCount;
    }

    public async Task SaveMealCountAsync(int count)
    {
        if (count < MinimumMealCount || count > MaximumMealCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        await using var db = await _contextFactory.CreateDbContextAsync();
        var settings = await db.AppSettings.FindAsync(1);
        if (settings == null)
        {
            settings = new AppSettings { Id = 1 };
            db.AppSettings.Add(settings);
        }

        settings.MealCount = count;
        await db.SaveChangesAsync();
        MealCount = count;
    }
}