using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Controls;

public partial class SettingsControls : ComponentBase
{
    [Inject] private SettingsStateService SettingsState { get; set; } = null!;

    private Task SaveSettings()
    {
        return SettingsState.RequestSaveAsync();
    }

    private Task ResetChanges()
    {
        return SettingsState.RequestResetAsync();
    }
}