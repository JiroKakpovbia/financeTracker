using trackr.Models;
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
					LoadingOverlay.IsVisible = true;
					MainContent.IsVisible = false;

					IServiceProvider? services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
					if (services?.GetService(typeof(DashboardViewModel)) is not DashboardViewModel viewModel)
					{
						return;
					}

					viewModel.ShowAlertRequested += OnShowAlertRequested;
					viewModel.ShowPromptRequested += OnShowPromptRequested;

					viewModel.ShowAddAccountFormRequested += ShowAddAccountForm;
					viewModel.ShowAccountOptionsFormRequested += ShowAccountOptionsForm;

					// Add event handler for the AddAccountClicked event of the AddAccountForm
					AddAccountForm.AddAccountClicked += async (sender, account) =>
					{
						if (BindingContext is DashboardViewModel vm)
						{
							await vm.HandleAddAccount(account);
							await AddAccountSheet.Hide();

						}
					};

					// Add event handler for the AccountOptionsClicked events of the DashboardViewModel
					AccountOptionsForm.RenameAccountClicked += async (sender, account) =>
					{
						if (BindingContext is DashboardViewModel vm)
						{
							await vm.HandleRenameAccount(account);
							await AccountOptionsSheet.Hide();
						}

					};

					AccountOptionsForm.ImportCSVClicked += async (sender, account) =>
					{
						if (BindingContext is DashboardViewModel vm)
						{
							await vm.HandleImportCSV(account);
							await AccountOptionsSheet.Hide();
						}
					};

					AccountOptionsForm.MoveAccountClicked += async (sender, account) =>
					{
						if (BindingContext is DashboardViewModel vm)
						{
							await vm.HandleMoveAccount(account);
							await AccountOptionsSheet.Hide();
						}
					};

					AccountOptionsForm.DeleteAccountClicked += async (sender, account) =>
					{
						if (BindingContext is DashboardViewModel vm)
						{
							await vm.HandleDeleteAccount(account);
							await AccountOptionsSheet.Hide();
						}
					};


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

				LoadingOverlay.IsVisible = false;
				MainContent.IsVisible = true;
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

		private async Task ShowAddAccountForm()
		{
			try
			{
				Console.WriteLine("Opening Add Account form...");

				AddAccountForm.Reset();
				await AddAccountSheet.Show();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error opening Add Account form: {ex.Message}\n");
			}

		}

		private async Task ShowAccountOptionsForm(BankAccount? account)
		{
			try
			{
				Console.WriteLine($"Opening account options for account: {account?.Name} (ID: {account?.Id})");

				if (account != null)
				{
					AccountOptionsForm.SetAccount(account);
					await AccountOptionsSheet.Show();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling menu action: {ex.Message}\n");
			}
		}
	}
}