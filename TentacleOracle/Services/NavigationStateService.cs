using Microsoft.AspNetCore.Components;

namespace TentacleOracle.Services;

public class NavigationStateService
{
    private object? _currentOwner;
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

    public void SetControlType<TControl>(object owner) where TControl : ComponentBase
    {
        ArgumentNullException.ThrowIfNull(owner);
        _currentOwner = owner;
        CurrentControlType = typeof(TControl);
    }

    public void SetNoControls(object owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        _currentOwner = owner;
        CurrentControlType = null;
    }

    public void ClearControls(object owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        
        if (!ReferenceEquals(_currentOwner, owner)) return;

        _currentOwner = null;
        CurrentControlType = null;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}