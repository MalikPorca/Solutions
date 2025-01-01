using Solutions.Models;
using Solutions.Services;

namespace Solutions.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly ISolutionService _solutionService;
    private readonly ICategoryService _categoryService;
    private User _currentUser;

    public ProfilePage(IAuthService authService, ISolutionService solutionService, ICategoryService categoryService)
    {
        InitializeComponent();
        _authService = authService;
        _solutionService = solutionService;
        _categoryService = categoryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserProfile();
    }

    private async Task LoadUserProfile()
    {
        try
        {
            _currentUser = await _authService.GetCurrentUserAsync();
            if (_currentUser == null)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            // Update profile information
            NameLabel.Text = $"{_currentUser.FirstName} {_currentUser.LastName}";
            EmailLabel.Text = _currentUser.Email;
            JoinDateLabel.Text = $"Member since: {_currentUser.JoinDate:MMM d, yyyy}";
            
            if (!string.IsNullOrEmpty(_currentUser.ProfileImage))
            {
                ProfileImage.Source = _currentUser.ProfileImage;
            }

            // Update statistics
            var solutions = await _solutionService.GetSolutionsAsync();
            var userSolutions = solutions.Where(s => s.AuthorName == _currentUser.Username).ToList();
            SolutionsCountLabel.Text = userSolutions.Count.ToString();

            var categories = userSolutions.Select(s => s.Category).Distinct();
            CategoriesCountLabel.Text = categories.Count().ToString();

            var tags = userSolutions.SelectMany(s => s.Tags ?? new List<string>()).Distinct();
            TagsCountLabel.Text = tags.Count().ToString();

            // Update last activity
            var lastSolution = userSolutions.OrderByDescending(s => s.CreatedDate).FirstOrDefault();
            LastActivityLabel.Text = lastSolution != null 
                ? lastSolution.CreatedDate.ToString("MMM d") 
                : "No activity";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load profile: " + ex.Message, "OK");
        }
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        var firstName = await DisplayPromptAsync("Edit Profile", "First Name:", initialValue: _currentUser.FirstName);
        if (firstName == null) return;

        var lastName = await DisplayPromptAsync("Edit Profile", "Last Name:", initialValue: _currentUser.LastName);
        if (lastName == null) return;

        var bio = await DisplayPromptAsync("Edit Profile", "Bio:", initialValue: _currentUser.Bio);
        if (bio == null) return;

        _currentUser.FirstName = firstName;
        _currentUser.LastName = lastName;
        _currentUser.Bio = bio;

        try
        {
            await _authService.UpdateProfileAsync(_currentUser);
            await LoadUserProfile();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var oldPassword = await DisplayPromptAsync("Change Password", "Current Password:", 
            keyboard: Keyboard.Text, maxLength: 50);
        if (string.IsNullOrEmpty(oldPassword)) return;

        var newPassword = await DisplayPromptAsync("Change Password", "New Password:", 
            keyboard: Keyboard.Text, maxLength: 50);
        if (string.IsNullOrEmpty(newPassword)) return;

        var confirmPassword = await DisplayPromptAsync("Change Password", "Confirm New Password:", 
            keyboard: Keyboard.Text, maxLength: 50);
        if (string.IsNullOrEmpty(confirmPassword)) return;

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "New passwords do not match", "OK");
            return;
        }

        try
        {
            await _authService.ChangePasswordAsync(oldPassword, newPassword);
            await DisplayAlert("Success", "Password changed successfully", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnProfileImageTapped(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet("Profile Picture", "Cancel", null, "Take Photo", "Choose from Gallery");
        
        // This is a placeholder for future implementation
        await DisplayAlert("Coming Soon", "This feature will be available in a future update", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
        if (!confirm) return;

        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
