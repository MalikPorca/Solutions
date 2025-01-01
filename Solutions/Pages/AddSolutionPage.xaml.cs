using Solutions.Models;
using Solutions.Services;

namespace Solutions.Pages;

[QueryProperty(nameof(ReturnRoute), "returnRoute")]
public partial class AddSolutionPage : ContentPage
{
    private readonly ISolutionService _solutionService;
    public string ReturnRoute { get; set; } = "..";

    public AddSolutionPage(ISolutionService solutionService)
    {
        InitializeComponent();
        _solutionService = solutionService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Clear any previous entries
        TitleEntry.Text = string.Empty;
        DescriptionEditor.Text = string.Empty;
        CategoryEntry.Text = string.Empty;
        TagsEntry.Text = string.Empty;
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
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to save solution", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
