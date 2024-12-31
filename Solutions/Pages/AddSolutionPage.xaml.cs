using Solutions.Models;
using Solutions.Services;

namespace Solutions.Pages;

public partial class AddSolutionPage : ContentPage
{
    private readonly ISolutionService _solutionService;

    public AddSolutionPage(ISolutionService solutionService)
    {
        InitializeComponent();
        _solutionService = solutionService;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Error", "Title is required", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            await DisplayAlert("Error", "Description is required", "OK");
            return;
        }

        var solution = new Solution
        {
            Title = TitleEntry.Text,
            Description = DescriptionEditor.Text,
            Category = CategoryEntry.Text,
            Tags = TagsEntry.Text?.Split(',').Select(t => t.Trim()).ToList() ?? new List<string>(),
            AuthorName = "Current User", // This will be replaced with actual user data when authentication is implemented
            CreatedDate = DateTime.Now
        };

        var success = await _solutionService.AddSolutionAsync(solution);
        
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
