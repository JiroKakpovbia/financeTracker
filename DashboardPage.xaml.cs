using Microsoft.Maui.ApplicationModel;
using System.Globalization;

namespace financeTracker;

public partial class DashboardPage : ContentPage
{
	private readonly CsvImportService _csvImportService = new();
    private List<Transaction> _transactions = new();
	
	public DashboardPage()
	{
		InitializeComponent();
	}

	private async void TDTap(object sender, EventArgs e)
	{
		try
		{
			// Try opening the TD app via a URI scheme
			var appUri = new Uri("td://"); // Or "tdbank://"
			bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

			if (canOpen)
			{
				await Launcher.Default.OpenAsync(appUri);
			}
			else
			{
				// open the website if the app isn't installed
				var webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
				await Launcher.Default.OpenAsync(webUri);
			}
		}
		catch (Exception ex)
		{
			// Optionally log or handle errors
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
				var transactions = csvService.ParseTransactions(stream);

				DisplayTransactions(transactions);
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Could not import file: {ex.Message}", "OK");
		}
	}

	private void DisplayTransactions(List<Transaction> transactions)
	{
		TransactionsStack.Children.Clear();

		BalanceLabel.Text = $"Balance: \n${transactions.First().Balance:C}";

		foreach (var transaction in transactions)
		{
			var transactionLayout = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
				},
				Padding = new Thickness(0, 4)
			};

			// Description and Date
			var leftStack = new VerticalStackLayout
			{
				Spacing = 0,
				Children =
				{
					new Label { Text = transaction.Description, FontSize = 14, FontAttributes = FontAttributes.Bold },
					new Label { Text = transaction.Date.ToShortDateString(), FontSize = 12, TextColor = Colors.Gray }
				}
			};
			
			// Amount
			var amountLabel = new Label
			{
				Text = transaction.Amount.ToString("C", CultureInfo.GetCultureInfo("en-US")),
				FontSize = 20,
				// TextColor = "{Binding Amount, Converter={StaticResource AmountColourConverter}}"
				FontAttributes = FontAttributes.Bold,
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.End,
				VerticalOptions = LayoutOptions.Center
			};

			transactionLayout.Add(leftStack, 0, 0);
			transactionLayout.Add(amountLabel, 1, 0);

			var border = new Border
			{
				Stroke = Colors.LightGray,
				Padding = 6,
				Margin = new Thickness(0, 4),
				Content = transactionLayout
			};

			TransactionsStack.Children.Add(border);
		}
	}
}
