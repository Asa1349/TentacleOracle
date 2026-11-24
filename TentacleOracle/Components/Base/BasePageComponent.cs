using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Base;


public abstract class BasePageComponent : ComponentBase, IDisposable
{
    [Inject] protected NavigationStateService NavStateService { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        NavStateService.SetNoControls(this);
        base.OnInitialized();
    }
    
    public virtual void Dispose()
    {
        NavStateService.ClearControls(this);
    }
}