using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace financeTracker;

public partial class DashboardPage : ContentPage
{
	private readonly CsvImportService _csvImportService = new();
	private List<BankAccount> bankAccounts = new();
	Dictionary<string, VerticalStackLayout> transactionStacks = new();
	Dictionary<string, Label> balanceLabels = new();
	
	public DashboardPage()
	{
		InitializeComponent();
	}

	private async void OnAddAccountClicked(object sender, EventArgs e)
	{
		var popup = new AddAccountPopup();
		var result = await this.ShowPopupAsync(popup);

		if (result is BankAccount newAccount)
		{
			if (bankAccounts.Any(a => (a.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(newAccount.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(newAccount.Type, StringComparison.OrdinalIgnoreCase))))
			{
				await DisplayAlert("Duplicate", "An account with this name, bank, and type already exists.", "OK");
				return;
			}

			bankAccounts.Add(newAccount);
			AddAccountUI(newAccount);
		}
	}

	private async void OnThreeDotsClicked(object sender, EventArgs e)
	{
		string accountId = (sender as ImageButton).ClassId;

		// Display Action Sheet
		string action = await DisplayActionSheet("Choose an action", "Cancel", null, "Import CSV", "Delete Account");

		switch (action)
		{
			case "Import CSV":
				await HandleCsvImport(accountId);
				break;
			case "Delete Account":
				HandleAccountDeletion(accountId);
				break;
		}
	}

	private void AddAccountUI(BankAccount account)
	{
		string logoFile = account.Bank.ToLower().Replace(" ", "") + ".png";
		string classId = $"{account.Bank}-{account.Type}-{account.Name}";

		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = 50 },
				new ColumnDefinition { Width = 100 },
				new ColumnDefinition { Width = GridLength.Auto }
			},
			Padding = 10
		};

		var threedots = new ImageButton
		{
			Source = "threedots.webp",
			WidthRequest = 25,
            HeightRequest = 25,
			HorizontalOptions = LayoutOptions.Center,
			ClassId = classId
		};

		threedots.Clicked += OnThreeDotsClicked;

		var accountName = new Label
		{
			FontSize = 15,
			Text = account.Name,
			TextColor = Color.FromArgb("#f9f7ff"),
			HorizontalOptions = LayoutOptions.Center
		};

		var image = new ImageButton
		{
			Source = logoFile,
			WidthRequest = 75,
			HorizontalOptions = LayoutOptions.Center,
			Aspect = Aspect.AspectFit
		};

		image.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => OnLogoTap(account.Bank, EventArgs.Empty)),
			AutomationId = account.Bank
		});

		// var importButton = new Button
		// {
		// 	Text = "Import CSV",
		// 	BackgroundColor = Color.FromArgb("#a188d8"),
		// 	FontSize = 15,
		// 	TextColor = Color.FromArgb("#f9f7ff"),
		// 	HorizontalOptions = LayoutOptions.Center,
		// 	ClassId = classId
		// };

		// importButton.Clicked += OnImportCsvClicked;

		var balanceLabel = new Label
		{
			FontSize = 25,
			TextColor = Color.FromArgb("#f9f7ff"),
			VerticalOptions = LayoutOptions.Center,
			// HorizontalOptions = LayoutOptions.End
		};

		grid.Add(threedots, 0, 0);
		grid.Add(image, 1, 0);
		grid.Add(accountName, 1, 1);
		// grid.Add(importButton, 1, 0);
		grid.Add(balanceLabel, 2, 0);

		var transactionStack = new VerticalStackLayout
		{
			Padding = 10,
			Spacing = 6
		};

		var scroll = new ScrollView
		{
			HeightRequest = 150,
			VerticalScrollBarVisibility = ScrollBarVisibility.Always,
			Content = transactionStack
		};

		BankListLayout.Children.Add(grid);
		BankListLayout.Children.Add(scroll);

		transactionStacks[classId] = transactionStack;
		balanceLabels[classId] = balanceLabel;
	}

	private async void OnLogoTap(object sender, EventArgs e)
	{
		try
		{
			var image = sender as TapGestureRecognizer;
			string selectedBank = image.AutomationId;

			Uri? appUri = null;
			Uri? webUri = null;

			if (selectedBank == "TD")
			{
				appUri = new Uri("td://");
				webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
			}
			else if (selectedBank == "CIBC")
			{
				appUri = new Uri("cibc://");
				webUri = new Uri("https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html#/accounts/credit-cards/2c01046615744246b6ecadead422be4ddefd7b72ac9a7f7912f70bb70ab89bbe");
			}
			else if (selectedBank == "Capital One")
			{
				appUri = new Uri("capitalone://");
				webUri = new Uri("https://myaccounts.capitalone.com/accountSummary");
			}

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
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to open URI: {ex.Message}");
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

			if (result != null)
			{
				using var stream = await result.OpenReadAsync();
				using var reader = new StreamReader(stream);

				var csvService = new CsvImportService();

				if (string.IsNullOrEmpty(accountId) || !transactionStacks.ContainsKey(accountId)) {
					await DisplayAlert("Error", "Unknown account selected.", "OK");
					return;
				}
				
				string selectedBank = accountId.Split('-')[0];
				csvService.BankMappings.TryGetValue(selectedBank, out var config);

				var transactions = csvService.ParseTransactions(stream, config);
				DisplayTransactions(transactions, accountId);
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Could not import file: {ex.Message}", "OK");
		}
	}

	private void HandleAccountDeletion(string accountId)
	{
		// Find the account in the list
		var account = bankAccounts.FirstOrDefault(a => a.Name.Equals(accountId, StringComparison.OrdinalIgnoreCase));

		// var confirmation = DisplayAlert("Confirm", $"Are you sure you want to delete\n{account.Name}?.", "Yes", "Cancel");

		if (true && account != null)
		{
			string classId = $"{account.Bank}-{account.Type}-{account.Name}";

			// Remove it from the internal list
			bankAccounts.Remove(account);

			// Remove UI elements
			// BankListLayout.Children.Add(grid);
			// BankListLayout.Children.Add(scroll);

			transactionStacks.Remove(classId);
			balanceLabels.Remove(classId);

			DisplayAlert("Deleted", $"{account.Name} has been deleted.", "OK");
		}
		else
		{
			Console.WriteLine($"Couldn't delete account");
		}
	}

	private void DisplayTransactions(List<Transaction> transactions, string accountId)
	{
		var targetStack = transactionStacks[accountId];
		targetStack.Children.Clear();

		var targetBalance = balanceLabels[accountId];
		targetBalance.Text = (accountId.Split('-')[1] == "Debit" || accountId.Split('-')[0] == "TD") ? $"Balance: ${transactions.First().Balance.ToString():C}" : $"Balance: ${(transactions.First().Balance * -1).ToString():C}";

		foreach (var transaction in transactions)
		{
			var transactionLayout = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
				},
				Padding = 10,
				BackgroundColor = Color.FromArgb("#1e1e21")
			};

			// Description and Date
			var leftStack = new VerticalStackLayout
			{
				Spacing = 0,
				Children =
				{
					new Label { Text = transaction.Description, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#f9f7ff") },
					new Label { Text = transaction.Date.ToShortDateString(), FontSize = 12, TextColor = Color.FromArgb("#a188d8") },
					// new Label { Text = (accountId.Split('-')[1] == "Debit" || accountId.Split('-')[0] == "TD") ? transaction.Balance.ToString() : (transaction.Balance * -1).ToString(), FontSize = 10, TextColor = Colors.Gray }
				}
			};
			
			// Amount
			var amountLabel = new Label
			{
				Text = (accountId.Split('-')[1] == "Debit" || accountId.Split('-')[0] == "TD") ? transaction.Amount.ToString("C", CultureInfo.GetCultureInfo("en-US")) : (transaction.Amount * -1).ToString("C", CultureInfo.GetCultureInfo("en-US")),
				FontSize = 20,
				TextColor = (transaction.Amount < 0) ? Colors.Red : Colors.Green,
				FontAttributes = FontAttributes.Bold,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center
			};

			transactionLayout.Add(leftStack, 0, 0);
			transactionLayout.Add(amountLabel, 1, 0);

			var border = new Border
			{
				Stroke = Color.FromArgb("#52446f"),
				Content = transactionLayout
			};

			targetStack.Children.Add(border);
		}
	}
}
