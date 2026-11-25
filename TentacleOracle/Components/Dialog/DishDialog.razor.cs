using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using TentacleOracle.Components.Base;
using TentacleOracle.Enums;
using TentacleOracle.Models;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Dialog;

public partial class DishDialog : LifetimeComponentBase
{
    private const string DropdownChevron =
        "<path d=\"M6 9l6 6 6-6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    private EditContext _editContext = null!;
    private string? _error;
    private DishInput _model = new();
    private bool _saving;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IDishService DishService { get; set; } = null!;

    [Inject] private ILogger<DishDialog> Logger { get; set; } = null!;

    [Parameter] public IReadOnlyList<string> Categories { get; set; } = [];

    [Parameter] public Dish? DishToEdit { get; set; }

    private bool IsEditing => DishToEdit != null;

    protected override void OnInitialized()
    {
        Reset();
    }

    private Task<IEnumerable<string>> SearchCategoriesAsync(string? searchText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var search = searchText?.Trim();
        var matches = string.IsNullOrEmpty(search)
            ? Categories
            : Categories.Where(category => category.Contains(search, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(matches);
    }

    private void Reset()
    {
        if (IsDisposed || _saving) return;
        _model = DishToEdit is { } dish
            ? new DishInput
            {
                Name = dish.Name,
                Category = dish.Category,
                Kcal = dish.Kcal,
                Effort = (DishEffort)dish.Effort,
                RatingA = (DishRating)dish.RatingA,
                RatingR = (DishRating)dish.RatingR
            }
            : new DishInput();
        _editContext = new EditContext(_model);
        _error = null;
    }

    private void Cancel()
    {
        if (!IsDisposed && !_saving) MudDialog.Cancel();
    }

    private async Task SaveAsync()
    {
        if (IsDisposed || _saving) return;
        _saving = true;
        _error = null;

        try
        {
            var dish = CreateDishFromInput();
            if (await TrySaveDishAsync(dish) && !IsDisposed)
                MudDialog.Close(DialogResult.Ok(dish));
        }
        finally
        {
            if (!IsDisposed) _saving = false;
        }
    }

    private Dish CreateDishFromInput() => new()
    {
        Id = DishToEdit?.Id ?? 0,
        Name = _model.Name.Trim(),
        Category = _model.Category.Trim(),
        Kcal = _model.Kcal,
        Effort = (int)_model.Effort,
        RatingA = (int)_model.RatingA,
        RatingR = (int)_model.RatingR
    };

    private async Task<bool> TrySaveDishAsync(Dish dish)
    {
        try
        {
            if (IsEditing)
                await DishService.UpdateDishAsync(dish, LifetimeToken);
            else
                await DishService.AddDishAsync(dish, LifetimeToken);
            return true;
        }
        catch (OperationCanceledException) when (LifetimeToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            if (IsDisposed) return false;
            Logger.LogError(exception, "Could not save dish {DishId} (editing: {IsEditing}).", dish.Id, IsEditing);
            _error = "The dish could not be saved. Please try again.";
            return false;
        }
    }

    private sealed class DishInput
    {
        [Required(ErrorMessage = "Please enter a name.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)] public string Category { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Calories cannot be negative.")]
        public int Kcal { get; set; }

        [EnumDataType(typeof(DishEffort))] public DishEffort Effort { get; set; }

        [EnumDataType(typeof(DishRating))] public DishRating RatingA { get; set; }

        [EnumDataType(typeof(DishRating))] public DishRating RatingR { get; set; }
    }
}