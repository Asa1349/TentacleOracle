using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Settings : BasePageWithControlsComponent<SettingsControls>
{
    private bool _busy;
    private EditContext _editContext = null!;
    private string? _error;

    private SettingsInput _model = new();
    private bool _saved;
    [Inject] private ILogger<Settings> Logger { get; set; } = null!;
    [Inject] private SettingsStateService SettingsState { get; set; } = null!;

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
        if (_busy) return;
        _saved = false;
        if (_editContext.Validate()) await SaveValidSettingsAsync();
        StateHasChanged();
    }

    private async Task SaveValidSettingsAsync()
    {
        if (_busy) return;
        _busy = true;
        _saved = false;
        _error = null;
        StateHasChanged();
        try
        {
            await SettingsState.SaveMealCountAsync(_model.MealCount);
            _saved = true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not save settings.");
            _error = "Settings could not be saved. Please try again.";
        }
        finally
        {
            _busy = false;
            StateHasChanged();
        }
    }

    private async Task ResetChangesAsync()
    {
        if (_busy) return;
        _busy = true;
        _saved = false;
        _error = null;
        StateHasChanged();
        try
        {
            await SettingsState.LoadAsync();
            ResetForm();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load settings.");
            _error = "Settings could not be loaded. Please try again.";
        }
        finally
        {
            _busy = false;
            StateHasChanged();
        }
    }

    private void ResetForm()
    {
        if (_editContext != null) _editContext.OnFieldChanged -= OnFieldChanged;
        _model = new SettingsInput { MealCount = SettingsState.MealCount };
        _editContext = new EditContext(_model);
        _editContext.OnFieldChanged += OnFieldChanged;
        _saved = false;
        StateHasChanged();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        _saved = false;
    }

    public override void Dispose()
    {
        SettingsState.SaveRequested -= SaveSettingsAsync;
        SettingsState.ResetRequested -= ResetChangesAsync;
        _editContext.OnFieldChanged -= OnFieldChanged;
        base.Dispose();
    }

    private sealed class SettingsInput
    {
        [Range(SettingsStateService.MinimumMealCount, SettingsStateService.MaximumMealCount,
            ErrorMessage = "Please enter a whole number between 1 and 28.")]
        public int MealCount { get; set; } = SettingsStateService.DefaultMealCount;
    }
}