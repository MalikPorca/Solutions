using Solutions.Models;
using Solutions.Services;
using System.Windows.Input;

namespace Solutions.Pages;

public partial class CategoriesPage : ContentPage
{
    private readonly ICategoryService _categoryService;
    public ICommand EditCommand { get; }

    public CategoriesPage(ICategoryService categoryService)
    {
        InitializeComponent();
        _categoryService = categoryService;
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

            var action = await DisplayActionSheet(
                selectedCategory.Name,
                "Cancel",
                selectedCategory.Name == "Other" ? null : "Delete",
                "Edit", "View Solutions");

            switch (action)
            {
                case "Edit":
                    OnEditCategory(selectedCategory);
                    break;
                case "Delete":
                    await DeleteCategory(selectedCategory);
                    break;
                case "View Solutions":
                    // We'll implement this later
                    await DisplayAlert("Coming Soon", "This feature will be available soon!", "OK");
                    break;
            }
        }
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
