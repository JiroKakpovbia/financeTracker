using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace financeTracker;

public partial class DashboardPage : ContentPage
{
	private readonly AccountDataService accountDataService = new();
	private List<BankAccount> bankAccounts = new();
	Dictionary<string, VerticalStackLayout> transactionStacks = new();
	Dictionary<string, Label> balanceLabels = new();
	Dictionary<string, ScrollView> scrollViews = new();

	public DashboardPage()
	{
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
		// Console.WriteLine("Start loading accounts...");

		bankAccounts = await accountDataService.LoadAccountsAsync();

		// Console.WriteLine($"Loaded {bankAccounts.Count} accounts");

		foreach (var account in bankAccounts)
		{
			// Console.WriteLine($"Adding UI for account: {account.Name}");
			AddAccountUI(account);

			string accountId = $"{account.Bank}-{account.Type}-{account.Name}";

			if (transactionStacks.ContainsKey(accountId))
			{
				// Console.WriteLine($"Displaying transactions for: {accountId}");
				DisplayTransactions(account.Transactions, accountId);
			}
		}

		// Console.WriteLine("Finished loading accounts.");
	}

	private void AddAccountUI(BankAccount account)
	{
		string logoFile = account.Bank.ToLower().Replace(" ", "") + ".png";
		string accountId = $"{account.Bank}-{account.Type}-{account.Name}";

		double width = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

		// Main grid for account
		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) }, // Three dots
    			new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) }, // Bank Logo
    			new ColumnDefinition { Width = new GridLength(0.40, GridUnitType.Star) }, // Account Name
    			new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) }  // Dropdown Arrow
},
			Padding = 10
		};

		// Three dots button
		var threedots = new ImageButton
		{
			Source = "threedots.webp",
			WidthRequest = 20,
			HeightRequest = 20,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			ClassId = accountId
		};

		threedots.Clicked += OnThreeDotsClicked;

		// Bank logo
		var image = new ImageButton
		{
			Source = logoFile,
			WidthRequest = 50,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Aspect = Aspect.AspectFit
		};

		image.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => OnLogoTap(account.Bank, EventArgs.Empty)),
			AutomationId = account.Bank
		});

		// Account name label
		var accountName = new Label
		{
			FontSize = 20,
			Text = account.Name,
			TextColor = Color.FromArgb("#f9f7ff"),
			VerticalOptions = LayoutOptions.Center
		};

		// Balance label
		var balanceLabel = new Label
		{
			FontSize = 20,
			Text = "Balance: $?",
			TextColor = Color.FromArgb("#f9f7ff"),
			VerticalOptions = LayoutOptions.Center
		};

		// Labels grid
		var labelsGrid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
		};

		labelsGrid.Add(accountName, 0, 0);
		labelsGrid.Add(balanceLabel, 0, 1);

		// Dropdown arrow
		var arrow = new ImageButton
		{
			Source = "arrow_down.png",
			WidthRequest = 20,
			HeightRequest = 20,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			ClassId = accountId
		};

		arrow.Clicked += OnArrowClicked;

		grid.Add(threedots, 0, 0);
		grid.Add(image, 1, 0);
		grid.Add(labelsGrid, 2, 0);
		grid.Add(arrow, 3, 0);

		var transactionStack = new VerticalStackLayout
		{
			Padding = 10,
			Spacing = 5
		};

		// Hidden ScrollView for transactions
		var scroll = new ScrollView
		{
			HeightRequest = 150,
			VerticalScrollBarVisibility = ScrollBarVisibility.Always,
			Content = transactionStack,
			IsVisible = false,
		};

		BankListLayout.Children.Add(grid);
		BankListLayout.Children.Add(scroll);

		// Separator line
		var separator = new BoxView
		{
			HeightRequest = 1,
			Color = Color.FromArgb("#454549"),
			HorizontalOptions = LayoutOptions.Fill,
			Margin = new Thickness(0, 10, 0, 10)
		};
		BankListLayout.Children.Add(separator);

		transactionStacks[accountId] = transactionStack;
		balanceLabels[accountId] = balanceLabel;
		scrollViews[accountId] = scroll;
	}

	private async Task HandleRenameAccount(string accountId)
	{
		var account = bankAccounts.FirstOrDefault(a => $"{a.Bank}-{a.Type}-{a.Name}" == accountId);

		if (account == null)
		{
			await DisplayAlert("Error", "Account not found.", "OK");
			return;
		}

		string newName = await DisplayPromptAsync("Rename Account", "Enter the new account name:", "OK", "Cancel", initialValue: account.Name);

		if (!string.IsNullOrWhiteSpace(newName))
		{
			// Ensure it's unique
			if (bankAccounts.Any(a => (a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(account.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(account.Type, StringComparison.OrdinalIgnoreCase))))
			{
				await DisplayAlert("Error", "An account with this name already exists.", "OK");
				return;
			}

			// Update account name
			account.Name = newName;

			// Update UI
			var targetGrid = BankListLayout.Children.OfType<Grid>().FirstOrDefault(g => g.Children.OfType<ImageButton>().Any(img => img.ClassId == accountId));
			if (targetGrid != null)
			{
				// Now we search for the labelsGrid inside the targetGrid
				var labelsGrid = targetGrid.Children.OfType<Grid>().FirstOrDefault();

				if (labelsGrid != null)
				{
					// Find the Label in the first row of the labelsGrid (accountName)
					var nameLabel = labelsGrid.Children.OfType<Label>().FirstOrDefault();
					if (nameLabel != null)
					{
						nameLabel.Text = newName;
					}
				}

				// Update ClassId for the three dots button to reflect the new name
				var threedots = targetGrid.Children.OfType<ImageButton>().FirstOrDefault();
				if (threedots != null)
				{
					threedots.ClassId = $"{account.Bank}-{account.Type}-{account.Name}";
				}

				await DisplayAlert("Success", $"Account renamed to '{newName}'.", "OK");
			}
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

				if (string.IsNullOrEmpty(accountId) || !transactionStacks.ContainsKey(accountId))
				{
					await DisplayAlert("Error", "Unknown account selected.", "OK");
					return;
				}

				string selectedBank = accountId.Split('-')[0];
				csvService.BankMappings.TryGetValue(selectedBank, out var config);

				var account = bankAccounts.FirstOrDefault(a => $"{a.Bank}-{a.Type}-{a.Name}" == accountId);

				if (account != null && config != null)
				{
					account.Transactions = csvService.ParseTransactions(stream, config);
					DisplayTransactions(account.Transactions, accountId);
				}

			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Could not import file: {ex.Message}", "OK");
		}
	}

	private async void HandleAccountDeletion(string accountId)
	{
		// Find the account in the list
		var account = bankAccounts.FirstOrDefault(a => $"{a.Bank}-{a.Type}-{a.Name}" == accountId);

		if (account != null)
		{
			var confirmation = await DisplayAlert("Confirm", $"Are you sure you want to delete\n{account.Name}?", "Yes", "Cancel");

			if (confirmation && account != null)
			{
				// Remove it from the internal list
				bankAccounts.Remove(account);

				// Clear from saved accounts
				await accountDataService.DeleteAccountAsync(accountId);

				// Remove UI elements
				var targetGrid = BankListLayout.Children.OfType<Grid>().FirstOrDefault(g => g.Children.OfType<ImageButton>().Any(dots => dots.ClassId == accountId));
				var targetScroll = transactionStacks[accountId];
				var parentScrollView = targetScroll.Parent as ScrollView;

				if (targetGrid != null)
				{
					BankListLayout.Children.Remove(targetGrid);
				}

				if (targetScroll != null)
				{
					targetScroll.Children.Clear();
					BankListLayout.Children.Remove(targetScroll);
				}

				if (parentScrollView != null)
				{
					BankListLayout.Children.Remove(parentScrollView);
				}

				transactionStacks.Remove(accountId);
				balanceLabels.Remove(accountId);

				await DisplayAlert("Deleted", $"Account \"{account.Name}\" has been deleted.", "OK");
			}
		}
	}

	private void DisplayTransactions(List<Transaction> transactions, string accountId)
	{
		if (transactions == null || transactions.Count == 0) return;

		var targetStack = transactionStacks[accountId];
		targetStack.Children.Clear();

		var targetBalance = balanceLabels[accountId];
		targetBalance.Text = (accountId.Split('-')[1] == "Debit" || accountId.Split('-')[0] == "TD") ? $"Balance: ${transactions.First().Balance.ToString():C}" : $"Balance: ${(transactions.First().Balance * -1).ToString():C}";

		var account = bankAccounts.FirstOrDefault(a => $"{a.Bank}-{a.Type}-{a.Name}" == accountId);

		if (account == null) return;

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

	private async void OnAddAccountClicked(object sender, EventArgs e)
	{
		var popup = new AddAccountPopup();
		var result = await this.ShowPopupAsync(popup);

		if (result == null)
		{
			await DisplayAlert("Error", "All fields are required. Please fill in all details to add an account.", "OK");
		}

		if (result is BankAccount newAccount)
		{
			if (bankAccounts.Any(a => a.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase) && a.Bank.Equals(newAccount.Bank, StringComparison.OrdinalIgnoreCase) && a.Type.Equals(newAccount.Type, StringComparison.OrdinalIgnoreCase)))
			{
				await DisplayAlert("Duplicate", "An account with this name, bank, and type already exists.", "OK");
				return;
			}

			bankAccounts.Add(newAccount);
			AddAccountUI(newAccount);
		}

		// Save accounts
		await accountDataService.SaveAccountsAsync(bankAccounts);
	}

	private async void OnThreeDotsClicked(object? sender, EventArgs e)
	{
		var account = sender as ImageButton;

		if (account != null)
		{
			string accountId = account.ClassId;

			// Display Action Sheet
			string action = await DisplayActionSheet("Options", "Cancel", null, "Rename Account", "Import CSV", "Delete Account");

			switch (action)
			{
				case "Rename Account":
					await HandleRenameAccount(accountId);
					break;
				case "Import CSV":
					await HandleCsvImport(accountId);
					break;
				case "Delete Account":
					HandleAccountDeletion(accountId);
					break;
			}

			// Save accounts
			await accountDataService.SaveAccountsAsync(bankAccounts);
		}
	}

	private void OnArrowClicked(object? sender, EventArgs e)
	{
		var account = sender as ImageButton;

		if (account != null)
		{
			string accountId = account.ClassId;
			var targetScroll = scrollViews[accountId];

			if (targetScroll.IsVisible)
			{
				targetScroll.IsVisible = false;
				account.Source = "arrow_down.png"; // collapsed version
			}
			else
			{
				targetScroll.IsVisible = true;
				account.Source = "arrow_up.png"; // expanded version
			}
		}
	}

	private async void OnLogoTap(object sender, EventArgs e)
	{
		try
		{
			var image = sender as TapGestureRecognizer;

			if (image != null)
			{
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

		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to open URI: {ex.Message}");
		}
	}
}
