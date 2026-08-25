using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using trackr.Services;

namespace trackr;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
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

        // Register AccountDataService and DashboardViewModel as singletons
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<AccountDataService>();
		builder.Services.AddSingleton<ViewModels.DashboardViewModel>();

		return builder.Build();
	}
}
