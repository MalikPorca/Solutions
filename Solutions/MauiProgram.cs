using Microsoft.Extensions.Logging;
using Solutions.Services;
using Solutions.Pages;

namespace Solutions;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ISolutionService, SolutionService>();
        
        // Register pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AddSolutionPage>();
        builder.Services.AddTransient<SolutionDetailPage>();
        builder.Services.AddTransient<EditSolutionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
