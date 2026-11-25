using MudBlazor;
using TentacleOracle.Components.Dialog;
using TentacleOracle.Enums;

namespace TentacleOracle.Extensions;

public static class DialogServiceExtensions
{
    /// <summary>
    /// Returns true for Yes or OK, false for No, and null when dismissed.
    /// </summary>
    public static async Task<bool?> ShowMessageAsync(this IDialogService dialogService,
        MessageBoxType type, string text, MessageBoxButtons buttons = MessageBoxButtons.Ok)
    {
        var parameters = new DialogParameters<MessageBoxDialog>
        {
            { dialog => dialog.Type, type },
            { dialog => dialog.Text, text },
            { dialog => dialog.Buttons, buttons }
        };
        var options = new DialogOptions
        {
            NoHeader = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small,
            BackdropClick = false,
            CloseOnEscapeKey = true
        };

        var dialog = await dialogService.ShowAsync<MessageBoxDialog>(type.ToString(), parameters, options);
        var result = await dialog.Result;
        return result is { Canceled: false, Data: bool confirmed } ? confirmed : null;
    }
}