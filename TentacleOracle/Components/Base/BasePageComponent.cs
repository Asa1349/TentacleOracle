using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Base;


public abstract class BasePageComponent : ComponentBase, IDisposable
{
    [Inject] protected NavigationStateService NavStateService { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        NavStateService.ClearControls(); 
        base.OnInitialized();
    }
    
    protected override async Task OnInitializedAsync()
    {
        NavStateService.ClearControls(); 
        await base.OnInitializedAsync();
    }

    public virtual void Dispose()
    {
        NavStateService.ClearControls();
    }
}