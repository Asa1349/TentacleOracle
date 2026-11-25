using Microsoft.AspNetCore.Components;

namespace TentacleOracle.Components.Base;

public abstract class LifetimeComponentBase : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _lifetime = new();

    protected LifetimeComponentBase()
    {
        LifetimeToken = _lifetime.Token;
    }

    protected CancellationToken LifetimeToken { get; }
    protected bool IsDisposed { get; private set; }

    public virtual void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        _lifetime.Cancel();
        _lifetime.Dispose();
        GC.SuppressFinalize(this);
    }

    protected void Refresh()
    {
        if (!IsDisposed) StateHasChanged();
    }
}