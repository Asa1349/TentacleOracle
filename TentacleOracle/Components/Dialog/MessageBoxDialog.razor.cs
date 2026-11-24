using Microsoft.AspNetCore.Components;
using MudBlazor;
using TentacleOracle.Models.Enums;

namespace TentacleOracle.Components.Dialog;

public partial class MessageBoxDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] [EditorRequired] public MessageBoxType Type { get; set; }

    [Parameter] [EditorRequired] public string Text { get; set; } = string.Empty;

    [Parameter] public MessageBoxButtons Buttons { get; set; } = MessageBoxButtons.Ok;

    private string Title => Type.ToString();

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Decline()
    {
        MudDialog.Close(DialogResult.Ok(false));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}