using Solutions.Models;
using Solutions.Services;
using System.Windows.Input;
using System.Collections.Generic;

namespace Solutions.Pages;

public partial class CategoriesPage : ContentPage
{
    private readonly ICategoryService _categoryService;
    private readonly ISolutionService _solutionService;
    public ICommand EditCommand { get; }

    public CategoriesPage(ICategoryService categoryService, ISolutionService solutionService)
    {
        InitializeComponent();
        _categoryService = categoryService;
        _solutionService = solutionService;
        EditCommand = new Command<Category>(OnEditCategory);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategories();
    }

    private async Task LoadCategories()
    {
        try
        {
            var categories = await _categoryService.GetCategoriesAsync();
            var allSolutions = await _solutionService.GetSolutionsAsync();
            
            // Update total solutions count
            TotalSolutionsLabel.Text = $"Total Solutions: {allSolutions.Count}";

            foreach (var category in categories)
            {
                // Get solution count for each category
                var solutions = await _solutionService.GetSolutionsByCategoryAsync(category.Name);
                category.SolutionCount = solutions.Count;
            }
            CategoriesCollection.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load categories: " + ex.Message, "OK");
        }
    }

    private async void OnAddCategoryClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("New Category", "Enter category name:", "OK", "Cancel");
        if (string.IsNullOrWhiteSpace(name))
            return;

        var description = await DisplayPromptAsync("Category Description", "Enter category description:", "OK", "Cancel");
        if (description == null) // User cancelled
            return;

        var category = new Category
        {
            Name = name,
            Description = description,
            IconName = "ellipsis" // Default icon
        };

        var success = await _categoryService.AddCategoryAsync(category);
        if (success)
        {
            await LoadCategories();
        }
        else
        {
            await DisplayAlert("Error", "Failed to create category", "OK");
        }
    }

    private async void OnViewAllSolutionsClicked(object sender, EventArgs e)
    {
        var solutions = await _solutionService.GetSolutionsAsync();
        var navigationParameter = new Dictionary<string, object>
        {
            { "FilteredSolutions", solutions },
            { "CategoryName", string.Empty }  // Empty string means "All Solutions"
        };
        
        await Shell.Current.GoToAsync("//MainPage", navigationParameter);
    }

    private async void OnEditCategory(Category category)
    {
        if (category.Name == "Other")
        {
            await DisplayAlert("Info", "The 'Other' category cannot be modified.", "OK");
            return;
        }

        var name = await DisplayPromptAsync("Edit Category", "Enter new name:", "OK", "Cancel", category.Name);
        if (string.IsNullOrWhiteSpace(name))
            return;

        var description = await DisplayPromptAsync("Edit Description", "Enter new description:", "OK", "Cancel", category.Description);
        if (description == null) // User cancelled
            return;

        category.Name = name;
        category.Description = description;

        var success = await _categoryService.UpdateCategoryAsync(category);
        if (success)
        {
            await LoadCategories();
        }
        else
        {
            await DisplayAlert("Error", "Failed to update category", "OK");
        }
    }

    private async void OnCategorySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Category selectedCategory)
        {
            CategoriesCollection.SelectedItem = null;

            // Get solutions for the selected category
            var solutions = await _solutionService.GetSolutionsByCategoryAsync(selectedCategory.Name);
            
            // Navigate to MainPage with filtered solutions
            var navigationParameter = new Dictionary<string, object>
            {
                { "FilteredSolutions", solutions },
                { "CategoryName", selectedCategory.Name }
            };
            
            await Shell.Current.GoToAsync("//MainPage", navigationParameter);
        }
    }

    private async void OnDeleteCategoryClicked(object sender, EventArgs e)
    {
        // This method is not implemented in the provided code edit
    }

    private async Task DeleteCategory(Category category)
    {
        if (category.Name == "Other")
        {
            await DisplayAlert("Info", "The 'Other' category cannot be deleted.", "OK");
            return;
        }

        var confirm = await DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete the category '{category.Name}'? All solutions in this category will be moved to 'Other'.",
            "Delete",
            "Cancel");

        if (confirm)
        {
            var success = await _categoryService.DeleteCategoryAsync(category.Id);
            if (success)
            {
                await LoadCategories();
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete category", "OK");
            }
        }
    }
}
