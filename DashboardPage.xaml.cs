using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace financeTracker;

public partial class DashboardPage : ContentPage
{
	private readonly AccountDataService accountDataService = new();
	public ObservableCollection<BankAccount> BankAccounts { get; set; } = new();
	public static readonly BindableProperty MoveModeProperty = BindableProperty.Create(nameof(MoveMode), typeof(bool), typeof(DashboardPage), false);

	public bool MoveMode
	{
		get => (bool)GetValue(MoveModeProperty);
		set => SetValue(MoveModeProperty, value);
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged(string propertyName) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	public DashboardPage()
	{
		BindingContext = this;
		MoveMode = false;
		InitializeComponent();
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

	private async Task HandleAccountMove(string accountId, int direction)
	{
		var index = BankAccounts.ToList().FindIndex(a => a.Id == accountId);

		if (index < 0 || index >= BankAccounts.Count)
		{
			await DisplayAlert("Error", "Account not found.", "OK");
			return;
		}

		if (direction < 0 && index == 0)
		{
			await DisplayAlert("Error", "Account is already at the top.", "OK");
			return;
		}

		if (direction > 0 && index >= BankAccounts.Count - 1)
		{
			await DisplayAlert("Error", "Account is already at the bottom.", "OK");
			return;
		}

		// Swap accounts
		var swapAccount = BankAccounts[index + direction];
		BankAccounts.RemoveAt(index + direction);
		BankAccounts.Insert(index, swapAccount);
		await accountDataService.SaveAccountsAsync(BankAccounts);
		return;
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

	public Command<string> ToggleTransactionsCommand => new(async (accountId) =>
	{
		var account = BankAccounts.FirstOrDefault(a => a.Id == accountId);
		if (account != null)
		{
			if (account.Transactions.Count == 0)
			{
				await DisplayAlert("No Transactions", "This account has no transactions to show. Import a CSV to populate this account.", "OK");
				return;
			}
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
					MoveMode = true;
					await DisplayAlert("Move Mode", "You can now move accounts up or down by clicking the arrows.\n\nPress the 'Done' button when finished.", "OK");
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

	public Command<string> MoveUpCommand => new(async (accountId) =>
	{
		HandleAccountMove(accountId, -1);
	});

	public Command<string> MoveDownCommand => new(async (accountId) =>
	{
		HandleAccountMove(accountId, 1);
	});

	public Command DoneMoveCommand => new(() =>
	{
		MoveMode = false;
	});

	public Command AddAccountCommand => new(async () =>
	{
		var popup = new AddAccountPopup();
		var result = await this.ShowPopupAsync(popup);

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

	public Command<string> LogoTapCommand => new(async (bankName) =>
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
