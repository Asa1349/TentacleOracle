using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TentacleOracle.Data;

namespace TentacleOracle.Tests.TestData;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private SqliteTestDatabase(SqliteConnection connection, Mock<IDbContextFactory<AppDbContext>> contextFactory)
    {
        _connection = connection;
        ContextFactory = contextFactory.Object;
    }

    public IDbContextFactory<AppDbContext> ContextFactory { get; }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    public static async Task<SqliteTestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var factory = new Mock<IDbContextFactory<AppDbContext>>(MockBehavior.Strict);
        factory.Setup(contextFactory => contextFactory.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken _) => Task.FromResult(new AppDbContext(options)));
        var database = new SqliteTestDatabase(connection, factory);
        await using var context = await factory.Object.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        return database;
    }
}