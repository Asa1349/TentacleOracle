using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Settings : BasePageWithControlsComponent<SettingsControls>
{
    private bool _busy;
    private EditContext? _editContext;
    private string? _error;

    private SettingsInput _model = new();
    private int _persistedMealCount = AppSettings.DefaultMealCount;
    private bool _saved;
    private bool _settingsLoaded;
    [Inject] private ILogger<Settings> Logger { get; set; } = null!;
    [Inject] private SettingsStateService SettingsState { get; set; } = null!;
    [Inject] private ISettingsService SettingsService { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ResetForm();
        SettingsState.SaveRequested += SaveSettingsAsync;
        SettingsState.ResetRequested += ResetChangesAsync;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ResetChangesAsync();
    }

    private async Task SaveSettingsAsync()
    {
        if (IsDisposed || _busy || !_settingsLoaded) return;
        _saved = false;
        if (_editContext?.Validate() == true) await SaveValidSettingsAsync();
        Refresh();
    }

    private async Task SaveValidSettingsAsync()
    {
        if (IsDisposed || _busy || !_settingsLoaded) return;
        _busy = true;
        _saved = false;
        _error = null;
        Refresh();
        try
        {
            var mealCount = _model.MealCount;
            await SettingsService.SaveMealCountAsync(mealCount, LifetimeToken);
            if (IsDisposed) return;
            _persistedMealCount = mealCount;
            _saved = true;
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not save settings.");
            _error = "Settings could not be saved. Please reload the page.";
        }
        finally
        {
            if (!IsDisposed)
            {
                _busy = false;
                Refresh();
            }
        }
    }

    private async Task ResetChangesAsync()
    {
        if (IsDisposed || _busy) return;
        _busy = true;
        _saved = false;
        _error = null;
        Refresh();
        try
        {
            var mealCount = await SettingsService.GetMealCountAsync(LifetimeToken);
            if (IsDisposed) return;
            _persistedMealCount = mealCount;
            _settingsLoaded = true;
            ResetForm();
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (IsDisposed) return;
            Logger.LogError(exception, "Could not load settings.");
            _error = "Settings could not be loaded. Please reload the page.";
        }
        finally
        {
            if (!IsDisposed)
            {
                _busy = false;
                Refresh();
            }
        }
    }

    private void ResetForm()
    {
        if (IsDisposed) return;
        if (_editContext != null) _editContext.OnFieldChanged -= OnFieldChanged;
        _model = new SettingsInput { MealCount = _persistedMealCount };
        _editContext = new EditContext(_model);
        _editContext.OnFieldChanged += OnFieldChanged;
        _saved = false;
        Refresh();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (IsDisposed) return;
        _saved = false;
    }

    public override void Dispose()
    {
        if (IsDisposed) return;
        SettingsState.SaveRequested -= SaveSettingsAsync;
        SettingsState.ResetRequested -= ResetChangesAsync;
        if (_editContext != null) _editContext.OnFieldChanged -= OnFieldChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class SettingsInput
    {
        [Range(AppSettings.MinimumMealCount, AppSettings.MaximumMealCount,
            ErrorMessage = "Please enter a whole number between 1 and 28.")]
        public int MealCount { get; set; } = AppSettings.DefaultMealCount;
    }
}