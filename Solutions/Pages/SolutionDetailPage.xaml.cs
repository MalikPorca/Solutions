using Solutions.Models;
using Solutions.Services;

namespace Solutions.Pages;

[QueryProperty(nameof(Solution), "Solution")]
public partial class SolutionDetailPage : ContentPage
{
    private readonly ISolutionService _solutionService;
    private Solution _solution;

    public Solution Solution
    {
        get => _solution;
        set
        {
            _solution = value;
            OnPropertyChanged();
            BindingContext = _solution;
        }
    }

    public SolutionDetailPage(ISolutionService solutionService)
    {
        InitializeComponent();
        _solutionService = solutionService;
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "Solution", Solution }
        };
        await Shell.Current.GoToAsync("EditSolutionPage", navigationParameter);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var delete = await DisplayAlert("Confirm Delete", 
            "Are you sure you want to delete this solution?", 
            "Yes", "No");

        if (delete)
        {
            var success = await _solutionService.DeleteSolutionAsync(Solution.Id);
            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete solution", "OK");
            }
        }
    }
}
