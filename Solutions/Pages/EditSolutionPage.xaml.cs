using Solutions.Models;
using Solutions.Services;

namespace Solutions.Pages;

[QueryProperty(nameof(Solution), "Solution")]
public partial class EditSolutionPage : ContentPage
{
    private readonly ISolutionService _solutionService;
    private Solution _solution;

    public string TagsText
    {
        get => string.Join(", ", _solution?.Tags ?? new List<string>());
        set
        {
            if (_solution != null)
            {
                _solution.Tags = value?.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList() ?? new List<string>();
            }
        }
    }

    public Solution Solution
    {
        get => _solution;
        set
        {
            _solution = value;
            OnPropertyChanged();
            BindingContext = this;
        }
    }

    public EditSolutionPage(ISolutionService solutionService)
    {
        InitializeComponent();
        _solutionService = solutionService;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_solution.Title))
        {
            await DisplayAlert("Error", "Title is required", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_solution.Description))
        {
            await DisplayAlert("Error", "Description is required", "OK");
            return;
        }

        var success = await _solutionService.UpdateSolutionAsync(_solution);
        
        if (success)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Failed to save solution", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
