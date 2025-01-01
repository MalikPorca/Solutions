using Solutions.Models;
using Solutions.Services;
using Solutions.Pages;

namespace Solutions;

public partial class MainPage : ContentPage
{
    private readonly ISolutionService _solutionService;
    private readonly IServiceProvider _serviceProvider;

    public MainPage(ISolutionService solutionService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _solutionService = solutionService;
        _serviceProvider = serviceProvider;
        LoadSolutions();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
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
        var addPage = _serviceProvider.GetService<AddSolutionPage>();
        await Navigation.PushAsync(addPage);
    }

    private async void OnSolutionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Solution solution)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Solution", solution }
            };
            await Shell.Current.GoToAsync("SolutionDetailPage", navigationParameter);
        }
    }
}
