using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace financeTracker;

public partial class DashboardPage : ContentPage
{
	private readonly CsvImportService _csvImportService = new();
	private List<BankAccount> _bankAccounts = new();
    private List<Transaction> _transactions = new();
	
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
			if (_bankAccounts.Any(account => account.Name.Equals(newAccount.Name, StringComparison.OrdinalIgnoreCase)))
			{
				DisplayAlert("Duplicate", "An account with this name already exists.", "OK");
				return;
			}

			_bankAccounts.Add(newAccount);
			AddAccountUI(newAccount);
		}
	}

	private void AddAccountUI(BankAccount account)
	{
		string logoFile = account.Bank.ToLower().Replace(" ", "") + ".png";
		string classId = account.Bank.Replace(" ", "");

		var grid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Auto }
			},
			Padding = 10
		};

		var image = new Image
		{
			Source = logoFile,
			WidthRequest = 75,
			Aspect = Aspect.AspectFit
		};
		image.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => OnLogoTap(account.Bank, EventArgs.Empty)),
			AutomationId = account.Bank
		});

		var importButton = new Button
		{
			Text = "Import CSV",
			BackgroundColor = Color.FromArgb("#a188d8"),
			FontSize = 15,
			TextColor = Color.FromArgb("#f9f7ff"),
			HorizontalOptions = LayoutOptions.Center,
			ClassId = classId
		};
		importButton.Clicked += OnImportCsvClicked;

		var balanceLabel = new Label
		{
			FontSize = 20,
			TextColor = Color.FromArgb("#f9f7ff"),
			HorizontalOptions = LayoutOptions.End
		};

		grid.Add(image, 0, 0);
		grid.Add(importButton, 1, 0);
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
			else if (selectedBank == "CapitalOne")
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

	private async void OnImportCsvClicked(object sender, EventArgs e)
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

				var button = sender as Button;
    			string selectedBank = button.ClassId;

				if (csvService.BankMappings.TryGetValue(selectedBank, out var config))
				{
					var transactions = csvService.ParseTransactions(stream, config);
					DisplayTransactions(transactions, selectedBank);
				}
				else
				{
					Console.WriteLine($"Unsupported Bank: {selectedBank}");
				}
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Could not import file: {ex.Message}", "OK");
		}
	}

	private void DisplayTransactions(List<Transaction> transactions, string selectedBank)
	{
		Dictionary<string, VerticalStackLayout> bankStacks = new()
		{
			{ "TD", TDTransactionsStack },
			{ "CIBC", CIBCTransactionsStack },
			{ "CapitalOne", CapitalOneTransactionsStack }
		};

		Dictionary<string, Label> bankBalanceLabels = new()
		{
			{ "TD", TDBalanceLabel },
			{ "CIBC", CIBCBalanceLabel },
			{ "CapitalOne", CapitalOneBalanceLabel }
		};

		var targetStack = bankStacks[selectedBank];
		targetStack.Children.Clear();

		var targetBalance = bankBalanceLabels[selectedBank];

		targetBalance.Text = (selectedBank == "TD") ? $"Balance: \n${transactions.First().Balance.ToString():C}" : $"Balance: \n${(transactions.First().Balance * -1).ToString():C}";

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
					// new Label { Text = (selectedBank == "TD") ? transaction.Balance.ToString() : (transaction.Balance * -1).ToString(), FontSize = 10, TextColor = Colors.Gray }
				}
			};
			
			// Amount
			var amountLabel = new Label
			{
				Text = (selectedBank == "TD") ? transaction.Amount.ToString("C", CultureInfo.GetCultureInfo("en-US")) : (transaction.Amount * -1).ToString("C", CultureInfo.GetCultureInfo("en-US")),
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
