using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using trackr.Models;
using trackr.Popups;
using trackr.ViewModels;

namespace trackr.Pages
{

	public partial class DashboardPage : ContentPage
	{
		public DashboardPage()
		{
			InitializeComponent();
			DashboardViewModel viewModel = Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(DashboardViewModel)) as DashboardViewModel;
			if (viewModel == null) throw new Exception("DashboardViewModel could not be resolved from DI. Ensure it is registered as a service.");
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
				if (BindingContext is DashboardViewModel model)
				{
					model.LoadAccountsCommand.Execute(null);
					foreach (BankAccount account in model.BankAccounts) account.ShowTransactions = false;
				}
			}
			finally
			{
				LoadingOverlay.IsVisible = false;
				MainComponent.IsVisible = true;
			}
		}

		private async Task<bool> OnShowAlertRequested(object? sender, DashboardViewModel.AlertEventArgs args)
		{
			if (args.Title.Contains("Confirm")) return await DisplayAlert(args.Title, args.Message, "Yes", "Cancel");
			else
			{
				await DisplayAlert(args.Title, args.Message, "OK");
				return true;
			}
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
			AddAccountPopup popup = new AddAccountPopup();
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
				if (BindingContext is DashboardViewModel model) model.ShowMenuCommand.Execute(account);
			}
		}

		private void OnLogoButtonClicked(object sender, EventArgs e)
		{
			if (sender is ImageButton button && button.BindingContext is BankAccount account)
			{
				if (BindingContext is DashboardViewModel model) model.LogoTapCommand.Execute(account);
			}
		}

		private void OnToggleTransactionsButtonClicked(object sender, EventArgs e)
		{
			if (sender is ImageButton button && button.BindingContext is BankAccount account)
			{
				if (BindingContext is DashboardViewModel model) model.ToggleTransactionsCommand.Execute(account);
			}
		}
	}
}