using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using trackr.Services;
using trackr.ViewModels;
using trackr.Factories;

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
                fonts.AddFont("FontAwesome-Solid.otf", "FontAwesomeSolid");
                // fonts.AddFont("FontAwesome-Regular.otf", "FontAwesomeRegular");
                // fonts.AddFont("FontAwesome-Brands.otf", "FontAwesomeBrands");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Register services for dependency injection
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<IAccountDataService, AccountDataService>();
        builder.Services.AddSingleton<ICSVImportService, CSVImportService>();

        // Register Dashboard view models for dependency injection
        builder.Services.AddSingleton<DashboardPageViewModel>();
        builder.Services.AddTransient<AddAccountViewModel>();
        builder.Services.AddTransient<AccountOptionsViewModel>();

        // Register Budget view models for dependency injection
        // builder.Services.AddSingleton<BudgetPageViewModel>();

        // Register Analytics view models for dependency injection
        // builder.Services.AddSingleton<AnalyticsPageViewModel>();

        // Register Search view models for dependency injection
        builder.Services.AddSingleton<TransactionDetailsViewModel>();
        builder.Services.AddTransient<CategorySelectorViewModel>();
        builder.Services.AddSingleton<SearchPageViewModel>();

        // Register Settings view models for dependency injection
        // builder.Services.AddSingleton<SettingsPageViewModel>();

        // Register the factories for dependency injection
        builder.Services.AddTransient<ITransactionViewModelFactory, TransactionViewModelFactory>();
        builder.Services.AddTransient<IBankAccountViewModelFactory, BankAccountViewModelFactory>();

        return builder.Build();
    }
}
