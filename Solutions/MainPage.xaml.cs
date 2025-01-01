using Solutions.Models;
using Solutions.Services;
using Solutions.Pages;

namespace Solutions;

[QueryProperty(nameof(FilteredSolutions), "FilteredSolutions")]
[QueryProperty(nameof(CategoryName), "CategoryName")]
public partial class MainPage : ContentPage
{
    private readonly ISolutionService _solutionService;
    private readonly IServiceProvider _serviceProvider;
    private List<Solution> _filteredSolutions;
    private string _categoryName;

    public List<Solution> FilteredSolutions
    {
        get => _filteredSolutions;
        set
        {
            _filteredSolutions = value;
            if (value != null)
            {
                SolutionsCollection.ItemsSource = value;
            }
        }
    }

    public string CategoryName
    {
        get => _categoryName;
        set
        {
            _categoryName = value;
            if (!string.IsNullOrEmpty(value))
            {
                Title = $"{value} Solutions";
            }
            else
            {
                Title = "All Solutions";
            }
        }
    }

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
        if (FilteredSolutions == null)
        {
            LoadSolutions();
        }
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
            if (FilteredSolutions != null)
            {
                SolutionsCollection.ItemsSource = FilteredSolutions;
            }
            else
            {
                LoadSolutions();
            }
            return;
        }

        var results = await _solutionService.SearchSolutionsAsync(searchTerm);
        if (FilteredSolutions != null)
        {
            // Filter search results by category if we're in category view
            results = results.Where(s => s.Category == CategoryName).ToList();
        }
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
