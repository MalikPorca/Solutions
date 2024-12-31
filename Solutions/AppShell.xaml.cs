using Solutions.Pages;

namespace Solutions;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("AddSolutionPage", typeof(AddSolutionPage));
        Routing.RegisterRoute("SolutionDetailPage", typeof(SolutionDetailPage));
        Routing.RegisterRoute("EditSolutionPage", typeof(EditSolutionPage));
    }
}
