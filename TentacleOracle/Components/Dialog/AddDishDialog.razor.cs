using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using TentacleOracle.Models;
using TentacleOracle.Models.Enums;
using TentacleOracle.Services;

namespace TentacleOracle.Components.Dialog;

public partial class AddDishDialog : ComponentBase
{
    private const string DropdownChevron = "<path d=\"M6 9l6 6 6-6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject]
    private IDishService DishService { get; set; } = null!;

    [Inject]
    private ILogger<AddDishDialog> Logger { get; set; } = null!;

    [Parameter]
    public IReadOnlyList<string> Categories { get; set; } = [];

    private DishInput _model = new();
    private EditContext _editContext = null!;
    private bool _saving;
    private string? _error;

    protected override void OnInitialized() => _editContext = new EditContext(_model);

    private Task<IEnumerable<string>> SearchCategoriesAsync(string? searchText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var search = searchText?.Trim();
        IEnumerable<string> matches = string.IsNullOrEmpty(search)
            ? Categories
            : Categories.Where(category => category.Contains(search, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(matches);
    }

    private void Clear()
    {
        if (_saving) return;
        _model = new DishInput();
        _editContext = new EditContext(_model);
        _error = null;
    }

    private void Cancel()
    {
        if (!_saving) MudDialog.Cancel();
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
        _saving = true;
        _error = null;

        var dish = new Dish
        {
            Name = _model.Name.Trim(),
            Category = (_model.Category ?? string.Empty).Trim(),
            Kcal = _model.Kcal,
            Effort = (int)_model.Effort,
            RatingA = (int)_model.RatingA,
            RatingR = (int)_model.RatingR
        };

        try
        {
            await DishService.AddDishAsync(dish);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not add dish.");
            _error = "The dish could not be saved. Please try again.";
            return;
        }
        finally
        {
            _saving = false;
        }

        MudDialog.Close(DialogResult.Ok(dish));
    }

    //TODO: Move this class to a separate file if it grows larger or is used in multiple places.
    private sealed class DishInput
    {
        [Required(ErrorMessage = "Please enter a name.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Calories cannot be negative.")]
        public int Kcal { get; set; }

        [EnumDataType(typeof(DishEffort))]
        public DishEffort Effort { get; set; }

        [EnumDataType(typeof(DishRating))]
        public DishRating RatingA { get; set; }

        [EnumDataType(typeof(DishRating))]
        public DishRating RatingR { get; set; }
    }
}

