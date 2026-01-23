using Microsoft.AspNetCore.Components;

namespace TentacleOracle.Services;

public class NavigationStateService
{
    private Type? _currentControlType;
    
    public Type? CurrentControlType
    {
        get => _currentControlType;
        private set
        {
            if (_currentControlType != value)
            {
                _currentControlType = value;
                NotifyStateChanged();
            }
        }
    }

    public event Action? OnChange;

    public void SetControlType<TControl>() where TControl : ComponentBase
    {
        CurrentControlType = typeof(TControl);
    }

    public void ClearControls()
    {
        CurrentControlType = null;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}