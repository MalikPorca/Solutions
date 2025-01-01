using Solutions.Services;

namespace Solutions.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly IAuthService _authService;

    public RegisterPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (!ValidateInputs())
            return;

        try
        {
            RegisterButton.IsEnabled = false;
            var user = await _authService.RegisterAsync(
                UsernameEntry.Text.Trim(),
                EmailEntry.Text.Trim(),
                PasswordEntry.Text,
                FirstNameEntry.Text.Trim(),
                LastNameEntry.Text.Trim()
            );
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            RegisterButton.IsEnabled = true;
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            DisplayAlert("Error", "Please enter a username", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !EmailEntry.Text.Contains("@"))
        {
            DisplayAlert("Error", "Please enter a valid email address", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text))
        {
            DisplayAlert("Error", "Please enter your first name", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastNameEntry.Text))
        {
            DisplayAlert("Error", "Please enter your last name", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            DisplayAlert("Error", "Please enter a password", "OK");
            return false;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            DisplayAlert("Error", "Passwords do not match", "OK");
            return false;
        }

        return true;
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
