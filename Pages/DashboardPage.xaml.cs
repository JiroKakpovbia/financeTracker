using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using financeTracker.Models;
using financeTracker.Popups;
using financeTracker.ViewModels;

namespace financeTracker.Pages
{

	public partial class DashboardPage : ContentPage
	{
		private readonly AccountDataService accountDataService = new();
		public ObservableCollection<BankAccount> BankAccounts { get; set; } = new();

		public DashboardPage()
		{
			InitializeComponent();
			var viewModel = new DashboardViewModel();
			viewModel.ShowAlertRequested += OnShowAlertRequested;
			viewModel.ShowPromptRequested += OnShowPromptRequested;
			viewModel.ShowActionSheetRequested += OnShowActionSheetRequested;
			viewModel.ShowAddAccountPopupRequested += OnShowAddAccountPopupRequested;
			BindingContext = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			LoadingOverlay.IsVisible = true;
			MainComponent.IsVisible = false;

			try
			{
				if (BindingContext is DashboardViewModel model) model.LoadAccountsCommand.Execute(null);
			}
			finally
			{
				LoadingOverlay.IsVisible = false;
				MainComponent.IsVisible = true;
			}
		}

		private async void OnShowAlertRequested(object? sender, DashboardViewModel.AlertEventArgs args)
		{
			await DisplayAlert(args.Title, args.Message, "OK");
		}

		private async Task<string?> OnShowPromptRequested(object? sender, DashboardViewModel.PromptEventArgs args)
		{
			return await DisplayPromptAsync(args.Title, args.Message, "OK", "Cancel", initialValue: args.InitialValue);
		}

		private async Task<string?> OnShowActionSheetRequested(object? sender, DashboardViewModel.ActionSheetEventArgs args)
		{
			return await DisplayActionSheet(args.Title, args.Cancel, args.Destruction, args.Options);
		}

		private async Task<BankAccount?> OnShowAddAccountPopupRequested()
		{
			var popup = new AddAccountPopup();
			return await this.ShowPopupAsync(popup) as BankAccount;
		}
		private void OnAddAccountButtonClicked(object sender, EventArgs e)
		{
			if (BindingContext is DashboardViewModel model) model.AddAccountCommand.Execute(null);
		}

		private void OnMenuButtonClicked(object sender, EventArgs e)
		{
			if (sender is ImageButton button && button.BindingContext is BankAccount account)
			{
				if (BindingContext is DashboardViewModel model) model.ShowMenuCommand.Execute(account.Id);
			}
		}

		private void OnLogoButtonClicked(object sender, EventArgs e)
		{
			if (sender is ImageButton button && button.BindingContext is BankAccount account)
			{
				if (BindingContext is DashboardViewModel model) model.LogoTapCommand.Execute(account.Bank);
			}
		}

		private void OnToggleTransactionsButtonClicked(object sender, EventArgs e)
		{
			if (sender is ImageButton button && button.BindingContext is BankAccount account)
			{
				if (BindingContext is DashboardViewModel model) model.ToggleTransactionsCommand.Execute(account.Id);
			}
		}
	}
}