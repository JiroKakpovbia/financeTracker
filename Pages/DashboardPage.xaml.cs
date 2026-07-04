using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
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
		private bool viewModelInitialized;

		public DashboardPage()
		{
			InitializeComponent();
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			InitializeViewModel();
		}

		private void InitializeViewModel()
		{
			if (viewModelInitialized)
			{
				return;
			}

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
			viewModelInitialized = true;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			InitializeViewModel();

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
			if (args.Title.Contains("Confirm")) return await DisplayAlertAsync(args.Title, args.Message, "Yes", "Cancel");
			else
			{
				await DisplayAlertAsync(args.Title, args.Message, "OK");
				return true;
			}
		}

		private async Task<string?> OnShowPromptRequested(object? sender, DashboardViewModel.PromptEventArgs args)
		{
			return await DisplayPromptAsync(args.Title, args.Message, "OK", "Cancel", initialValue: args.InitialValue);
		}

		private async Task<string?> OnShowActionSheetRequested(object? sender, DashboardViewModel.ActionSheetEventArgs args)
		{
			return await DisplayActionSheetAsync(args.Title, args.Cancel, args.Destruction, args.Options);
		}

		private async Task<BankAccount?> OnShowAddAccountPopupRequested()
		{
			AddAccountPopup popup = new();
			var result = await this.ShowPopupAsync<BankAccount>(popup);
			return result.Result;
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