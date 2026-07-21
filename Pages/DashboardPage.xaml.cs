using CommunityToolkit.Maui.Extensions;
using trackr.Models;
using trackr.Popups;
using trackr.ViewModels;

namespace trackr.Pages
{

	public partial class DashboardPage : ContentPage
	{
		private bool viewModelInitialized;

		public DashboardPage()
		{
			InitializeComponent();
		}

		private async void InitializeViewModel()
		{
			Console.WriteLine("Initializing DashboardViewModel...");
			try
			{
				if (!viewModelInitialized)
				{
					IServiceProvider? services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
					if (services?.GetService(typeof(DashboardViewModel)) is not DashboardViewModel viewModel)
					{
						return;
					}

					viewModel.ShowAlertRequested += OnShowAlertRequested;
					viewModel.ShowPromptRequested += OnShowPromptRequested;
					viewModel.ShowActionSheetRequested += OnShowActionSheetRequested;
					viewModel.ShowAddAccountPopupRequested += OnShowAddAccountPopupRequested;
					BindingContext = viewModel;

					if (BindingContext is DashboardViewModel model)
					{
						await model.LoadAccountsAsync();

						foreach (BankAccount account in model.BankAccounts)
						{
							account.ShowTransactions = false;
						}
					}

					viewModelInitialized = true;

					Console.WriteLine("DashboardViewModel initialized successfully.\n");
				}
				else
				{
					Console.WriteLine("DashboardViewModel is already initialized. Skipping initialization.\n");
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error initializing DashboardViewModel: {ex.Message}\n");
				await DisplayAlertAsync("Error", "An unexpected error occurred while initializing the dashboard.", "OK");
			}
		}

		protected override async void OnAppearing()
		{
			Console.WriteLine("DashboardPage appearing...");
			try
			{
				base.OnAppearing();
				InitializeViewModel();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in OnAppearing: {ex.Message}\n");
				await DisplayAlertAsync("Error", "An unexpected error occurred while loading the dashboard.", "OK");
			}
		}

		private async Task<bool> OnShowAlertRequested(object? sender, DashboardViewModel.AlertEventArgs args)
		{
			try
			{
				if (args.Title.Contains("Confirm")) return await DisplayAlertAsync(args.Title, args.Message, "Yes", "Cancel");
				else
				{
					await DisplayAlertAsync(args.Title, args.Message, "OK");
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error showing alert: {ex.Message}\n");
				await DisplayAlertAsync("Error", "An unexpected error occurred while displaying an alert.", "OK");
				return false;
			}
		}

		private async Task<string?> OnShowPromptRequested(object? sender, DashboardViewModel.PromptEventArgs args)
		{
			try
			{
				return await DisplayPromptAsync(args.Title, args.Message, "OK", "Cancel", initialValue: args.InitialValue);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error showing prompt: {ex.Message}\n");
				await DisplayAlertAsync("Error", "An unexpected error occurred while displaying a prompt.", "OK");
				return null;
			}
		}

		private async Task<string?> OnShowActionSheetRequested(object? sender, DashboardViewModel.ActionSheetEventArgs args)
		{
			try
			{
				return await DisplayActionSheetAsync(args.Title, args.Cancel, args.Destruction, args.Options);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error showing action sheet: {ex.Message}\n");
				await DisplayAlertAsync("Error", "An unexpected error occurred while displaying an action sheet.", "OK");
				return null;
			}
		}

		private async Task<BankAccount?> OnShowAddAccountPopupRequested()
		{
			try
			{
				AddAccountPopup popup = new();
				var result = await this.ShowPopupAsync<BankAccount>(popup);
				return result.Result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error showing add account popup: {ex.Message}\n");
				await DisplayAlertAsync("Error", "An unexpected error occurred while displaying the add account popup.", "OK");
				return null;
			}
		}
	}
}