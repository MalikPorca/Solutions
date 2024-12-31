using Solutions.Models;
using Solutions.Services;

namespace Solutions;

public partial class MainPage : ContentPage
{
    private readonly ISolutionService _solutionService;

    public MainPage(ISolutionService solutionService)
    {
        InitializeComponent();
        _solutionService = solutionService;
        LoadSolutions();
    }

    private async void LoadSolutions()
    {
        var solutions = await _solutionService.GetSolutionsAsync();
        SolutionsCollection.ItemsSource = solutions;
    }

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        var searchTerm = SearchBar.Text;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            LoadSolutions();
            return;
        }

        var results = await _solutionService.SearchSolutionsAsync(searchTerm);
        SolutionsCollection.ItemsSource = results;
    }

    private async void OnAddSolutionClicked(object sender, EventArgs e)
    {
        // This will be implemented when we create the AddSolutionPage
        await Shell.Current.GoToAsync("AddSolutionPage");
    }

    private async void OnSolutionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Solution solution)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Solution", solution }
            };
            // This will be implemented when we create the SolutionDetailPage
            await Shell.Current.GoToAsync("SolutionDetailPage", navigationParameter);
        }
    }
}
