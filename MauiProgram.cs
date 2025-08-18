using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace financeTracker;

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
	builder.Services.AddSingleton<financeTracker.AccountDataService>();
	builder.Services.AddSingleton<financeTracker.ViewModels.DashboardViewModel>();

		return builder.Build();
	}
}
