using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddMudServices();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<DishStateService>();
builder.Services.AddScoped<NavigationStateService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        
        var dbConnectionString = dbContext.Database.GetConnectionString();
        if (!string.IsNullOrEmpty(dbConnectionString))
        {
            var dataSourcePrefix = "Data Source=";
            var startIndex = dbConnectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                var dbPath = dbConnectionString[(startIndex + dataSourcePrefix.Length)..];
                var semicolonIndex = dbPath.IndexOf(';');
                if (semicolonIndex >= 0)
                {
                    dbPath = dbPath[..semicolonIndex];
                }
                
                var directory = Path.GetDirectoryName(dbPath);
                
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    logger.LogInformation("Database directory created: {Directory}", directory);
                }
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