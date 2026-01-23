using Microsoft.AspNetCore.Components;
using TentacleOracle.Services;

namespace TentacleOracle.Base;

public abstract class BasePageWithControlsComponent<TControls> : ComponentBase, IDisposable
    where TControls : ComponentBase
{
    [Inject]
    protected NavigationStateService NavStateService { get; set; } = default!;

    protected override void OnInitialized()
    {
        NavStateService.SetControlType<TControls>();
        base.OnInitialized();
    }
    
    protected override async Task OnInitializedAsync()
    {
        NavStateService.SetControlType<TControls>();
        await base.OnInitializedAsync();
    }

    public virtual void Dispose()
    {
        NavStateService.ClearControls();
    }
}
