using Solutions.Pages;
using Solutions.Services;

namespace Solutions;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("SolutionDetailPage", typeof(SolutionDetailPage));
        Routing.RegisterRoute("EditSolutionPage", typeof(EditSolutionPage));
    }

    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        // If we're navigating to a page in the LoggedIn section, check authentication
        if (args.Target.Location.ToString().Contains("LoggedIn") && 
            !args.Target.Location.ToString().Contains("LoginPage"))
        {
            var authService = Handler.MauiContext?.Services.GetService<IAuthService>();
            if (authService != null && !authService.IsAuthenticated)
            {
                args.Cancel();
                await GoToAsync("//LoginPage");
            }
        }
    }
}
