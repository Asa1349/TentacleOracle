using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Base;

public abstract class BasePageWithControlsComponent<TControls> : ComponentBase, IDisposable
    where TControls : ComponentBase
{
    [Inject]
    protected NavigationStateService NavStateService { get; set; } = null!;

    protected override void OnInitialized()
    {
        NavStateService.SetControlType<TControls>(this);
        base.OnInitialized();
    }
    
    public virtual void Dispose()
    {
        NavStateService.ClearControls(this);
    }
}
