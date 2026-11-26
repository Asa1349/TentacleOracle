using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TentacleOracle.Components.Pages;
using TentacleOracle.Models;
using TentacleOracle.Services;
using TestContext = Bunit.TestContext;

namespace TentacleOracle.Tests.Components;

[TestFixture]
public sealed class SettingsTests
{
    [SetUp]
    public void SetUp()
    {
        _context = new TestContext();
        _settingsService = new Mock<ISettingsService>(MockBehavior.Strict);
        _settingsState = new SettingsStateService();

        _settingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AppSettings.DefaultMealCount);
        _settingsService.Setup(service => service.SaveMealCountAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _context.Services.AddSingleton(_settingsService.Object);
        _context.Services.AddSingleton(_settingsState);
        _context.Services.AddSingleton<NavigationStateService>();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private TestContext _context = null!;
    private Mock<ISettingsService> _settingsService = null!;
    private SettingsStateService _settingsState = null!;

    [Test]
    public void Render_LoadsPersistedMealCount()
    {
        _settingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);

        var cut = _context.RenderComponent<Settings>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cut.Find("#meal-count").GetAttribute("value"), Is.EqualTo("12"));
            Assert.That(cut.Find("#meal-count").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Markup, Does.Contain("Choose 1–28 meals."));
        }
    }

    [Test]
    public async Task SaveRequest_ValidInput_PersistsValueAndShowsSuccess()
    {
        var cut = _context.RenderComponent<Settings>();
        cut.Find("#meal-count").Change("14");

        await cut.InvokeAsync(() => _settingsState.RequestSaveAsync());

        _settingsService.Verify(service => service.SaveMealCountAsync(14, It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(cut.Find("[role='status']").TextContent, Is.EqualTo("Settings saved."));
    }

    [Test]
    public async Task SaveRequest_InvalidInput_DoesNotPersistAndShowsValidationMessage()
    {
        var cut = _context.RenderComponent<Settings>();
        cut.Find("#meal-count").Change("29");

        await cut.InvokeAsync(() => _settingsState.RequestSaveAsync());

        _settingsService.Verify(service => service.SaveMealCountAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.That(cut.Markup, Does.Contain("Please enter a whole number between 1 and 28."));
    }

    [Test]
    public async Task ResetRequest_RestoresPersistedValue()
    {
        _settingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(9);
        var cut = _context.RenderComponent<Settings>();
        cut.Find("#meal-count").Change("14");

        await cut.InvokeAsync(() => _settingsState.RequestResetAsync());

        Assert.That(cut.Find("#meal-count").GetAttribute("value"), Is.EqualTo("9"));
        _settingsService.Verify(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public void LoadFailure_ShowsReloadGuidanceAndDisablesInput()
    {
        _settingsService.Setup(service => service.GetMealCountAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        var cut = _context.RenderComponent<Settings>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cut.Find("[role='alert']").TextContent,
                Is.EqualTo("Settings could not be loaded. Please reload the page."));
            Assert.That(cut.Find("#meal-count").HasAttribute("disabled"), Is.True);
        }
    }

    [Test]
    public async Task SaveFailure_ShowsReloadGuidance()
    {
        _settingsService.Setup(service => service.SaveMealCountAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var cut = _context.RenderComponent<Settings>();
        cut.Find("#meal-count").Change("10");

        await cut.InvokeAsync(() => _settingsState.RequestSaveAsync());

        Assert.That(cut.Find("[role='alert']").TextContent,
            Is.EqualTo("Settings could not be saved. Please reload the page."));
    }
}