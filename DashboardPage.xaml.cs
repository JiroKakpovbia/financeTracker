using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;

namespace financeTracker;

public partial class DashboardPage : ContentPage
{
	private readonly AccountDataService accountDataService = new();
	public ObservableCollection<BankAccount> BankAccounts { get; set; } = new();
	private bool moveMode = false;

	public DashboardPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		LoadingOverlay.IsVisible = true;
		MainComponent.IsVisible = false;

		try
		{
			await LoadAccountsAsync();
		}
		finally
		{
			LoadingOverlay.IsVisible = false;
			MainComponent.IsVisible = true;
		}
	}

	private async Task LoadAccountsAsync()
	{
		var accounts = await accountDataService.LoadAccountsAsync();

		foreach (var account in accounts)
			BankAccounts.Add(account);
	}

	private async Task HandleAccountRenaming(string accountId)
	{
		var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

		if (account == null)
		{
			await DisplayAlert("Error", "Account not found.", "OK");
			return;
		}

		string newName = await DisplayPromptAsync("Rename Account", "Enter the new account name:", "OK", "Cancel", initialValue: account.Name);

		if (!string.IsNullOrWhiteSpace(newName))
		{
			// Ensure it's unique
			if (BankAccounts.Any(a => a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(account.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(account.Type, StringComparison.OrdinalIgnoreCase)))
			{
				await DisplayAlert("Error", "An account with this name, bank, and type already exists.", "OK");
				return;
			}

			// Update account name
			account.Name = newName;

			await DisplayAlert("Success", $"Account renamed to '{newName}'.", "OK");
		}
	}

	private async Task HandleCsvImport(string accountId)
	{
		try
		{
			var result = await FilePicker.PickAsync(new PickOptions
			{
				PickerTitle = "Select a CSV file",
				FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
				{
					{ DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } },
					{ DevicePlatform.Android, new[] { "text/csv" } },
					{ DevicePlatform.WinUI, new[] { ".csv" } },
					{ DevicePlatform.macOS, new[] { "csv" } },
				})
			});

			if (result == null)
			{
				Console.WriteLine("File selection was cancelled.");
				return;
			}
			using var stream = await result.OpenReadAsync();
			using var reader = new StreamReader(stream);

			var csvService = new CsvImportService();

			if (string.IsNullOrEmpty(accountId))
			{
				await DisplayAlert("Error", "Unknown account selected.", "OK");
				return;
			}

			var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

			csvService.BankMappings.TryGetValue(account.Bank, out var config);
			if (config == null)
			{
				await DisplayAlert("Error", $"No CSV configuration found for {account.Bank}.", "OK");
				return;
			}

			account.Transactions = csvService.ParseTransactions(stream, config);
			account.Balance = account.Transactions.FirstOrDefault()?.Balance ?? 0;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Could not import file: {ex.Message}", "OK");
		}
	}

	private async Task HandleAccountDeletion(string accountId)
	{
		// Find the account in the list
		var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

		if (account != null)
		{
			var confirmation = await DisplayAlert("Confirm", $"Are you sure you want to delete\n{account.Name}?", "Yes", "Cancel");

			if (confirmation && account != null)
			{
				// Remove it from the internal list
				BankAccounts.Remove(account);

				// Clear from saved accounts
				await accountDataService.DeleteAccountAsync(account.Id);

				await DisplayAlert("Deleted", $"'{account.Name}' has been deleted.", "OK");
			}
		}
	}

	private async Task HandleAccountMove(string accountId, int direction)
	{
		// moveMode = true;

		// var index = bankAccounts.FindIndex(a => a.Id == accountId);
		// if (index < 0) return;

		// int newIndex = index + direction;
		// if (newIndex < 0 || newIndex >= bankAccounts.Count) return;

		// // Swap in the data list
		// var temp = bankAccounts[index];
		// bankAccounts[index] = bankAccounts[newIndex];
		// bankAccounts[newIndex] = temp;

		// // Rebuild the UI
		// BankListLayout.Children.Clear();
		// foreach (var account in bankAccounts)
		// 	AddAccountUI(account);

		// // Optionally save the new order
		// await accountDataService.SaveAccountsAsync(bankAccounts);
		return;
	}

	public Command AddAccountCommand => new(async () =>
	{
		var popup = new AddAccountPopup();
		var result = await this.ShowPopupAsync(popup);

		if (result == null)
		{
			await DisplayAlert("Error", "All fields are required. Please fill in all details to add an account.", "OK");
		}

		if (result is BankAccount newAccount)
		{
			if (BankAccounts.Any(a => a.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(newAccount.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(newAccount.Type, StringComparison.OrdinalIgnoreCase)))
			{
				await DisplayAlert("Duplicate", "An account with this name, bank, and type already exists.", "OK");
				return;
			}

			BankAccounts.Add(newAccount);
		}

		// Save accounts
		await accountDataService.SaveAccountsAsync(BankAccounts);
	});

	public Command<string> ToggleTransactionsCommand => new(async (accountId) =>
	{
		var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);
		if (account != null)
		{
			account.ShowTransactions = !account.ShowTransactions;
		}
	});

	public Command<string> ShowMenuCommand => new(async (accountId) =>
	{
		var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);

		if (account != null)
		{
			string action = await DisplayActionSheet("Options", "Cancel", null, "Rename Account", "Move Account", "Import CSV", "Delete Account");

			switch (action)
			{
				case "Rename Account":
					await HandleAccountRenaming(accountId);
					break;
				case "Move Account":
					// moveMode = true;
					// BankListLayout.Children.Clear();
					// foreach (var acc in bankAccounts)
					// 	AddAccountUI(acc);
					break;
				case "Import CSV":
					await HandleCsvImport(accountId);
					break;
				case "Delete Account":
					HandleAccountDeletion(accountId);
					break;
			}

			// Save accounts
			await accountDataService.SaveAccountsAsync(BankAccounts);
		}
		return;
	});

	private Command<string> LogoTapCommand => new(async (bankName) =>
	{
		try
		{
			Uri? appUri = null;
			Uri? webUri = null;

			if (bankName == "TD")
			{
				appUri = new Uri("td://");
				webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
			}
			else if (bankName == "CIBC")
			{
				appUri = new Uri("cibc://");
				webUri = new Uri("https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html#/accounts/credit-cards/2c01046615744246b6ecadead422be4ddefd7b72ac9a7f7912f70bb70ab89bbe");
			}
			else if (bankName == "Capital One")
			{
				appUri = new Uri("capitalone://");
				webUri = new Uri("https://myaccounts.capitalone.com/accountSummary");
			}

			if (appUri != null && webUri != null)
			{
				// Try opening the App via a URI scheme
				bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

				if (canOpen)
				{
					await Launcher.Default.OpenAsync(appUri);
				}
				else
				{
					// open the website if the app isn't installed
					await Launcher.Default.OpenAsync(webUri);
				}
			}

		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to open URI: {ex.Message}");
		}
	});
}
