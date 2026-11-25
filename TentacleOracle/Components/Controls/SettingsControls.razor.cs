using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Controls;

public partial class SettingsControls : ComponentBase
{
    [Inject] private SettingsStateService SettingsState { get; set; } = null!;

    private void SaveSettings()
    {
        SettingsState.RequestSave();
    }

    private void ResetChanges()
    {
        SettingsState.RequestReset();
    }
}