using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using TentacleOracle.BusinessLogic;
using TentacleOracle.Components;
using TentacleOracle.Data;
using TentacleOracle.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
var isFileDatabase = sqliteBuilder.Mode != SqliteOpenMode.Memory
                     && !string.IsNullOrWhiteSpace(sqliteBuilder.DataSource)
                     && sqliteBuilder.DataSource != ":memory:";
if (isFileDatabase && !Path.IsPathRooted(sqliteBuilder.DataSource))
{
    sqliteBuilder.DataSource =
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, sqliteBuilder.DataSource));
}

var resolvedConnectionString = sqliteBuilder.ToString();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(resolvedConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddMudServices();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IMealPlanGenerator, MealPlanGenerator>();
builder.Services.AddTransient<MealPlanService>();
builder.Services.AddScoped<DishStateService>();
builder.Services.AddScoped<SettingsStateService>();
builder.Services.AddScoped<NavigationStateService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();

        if (isFileDatabase)
        {
            var directory = Path.GetDirectoryName(sqliteBuilder.DataSource);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                logger.LogInformation("Database directory created: {Directory}", directory);
            }
        }

        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();