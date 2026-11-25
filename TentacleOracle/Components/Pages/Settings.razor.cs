using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TentacleOracle.Components.Base;
using TentacleOracle.Components.Controls;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Pages;

public partial class Settings : BasePageWithControlsComponent<SettingsControls>
{
    private EditContext _editContext = null!;

    private SettingsInput _model = new();
    private bool _saved;
    [Inject] private SettingsStateService SettingsState { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ResetChanges();
        SettingsState.SaveRequested += SaveSettings;
        SettingsState.ResetRequested += ResetChanges;
    }

    private void SaveSettings()
    {
        _saved = false;
        if (_editContext.Validate()) SaveValidSettings();
        StateHasChanged();
    }

    private void SaveValidSettings()
    {
        SettingsState.SaveMealCount(_model.MealCount);
        _saved = true;
    }

    private void ResetChanges()
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
        SettingsState.SaveRequested -= SaveSettings;
        SettingsState.ResetRequested -= ResetChanges;
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