using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using trackr.Services;
using trackr.ViewModels;

namespace trackr;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Register services for dependency injection
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IAccountDataService, AccountDataService>();
        builder.Services.AddSingleton<ICSVImportService, CSVImportService>();

        // Register view models for dependency injection
        builder.Services.AddSingleton<DashboardPageViewModel>();
        builder.Services.AddTransient<AddAccountViewModel>();
        builder.Services.AddTransient<AccountOptionsViewModel>();

        return builder.Build();
    }
}
