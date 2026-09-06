using CommunityToolkit.Maui.Extensions;
using trackr.Views;
using trackr.ViewModels;
using CommunityToolkit.Maui;

namespace trackr.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private bool isInitialized;

        // Constructor for DashboardPage
        public DashboardPage()
        {
            InitializeComponent();
        }

        // Initialize the DashboardPageViewModel and set up event handlers
        private async Task InitializeViewModelAsync()
        {
            Console.WriteLine("Initializing DashboardPageViewModel...");

            LoadingOverlay.IsVisible = true;
            MainContent.IsVisible = false;

            try
            {
                IServiceProvider? services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;

                if (services?.GetService(typeof(DashboardPageViewModel)) is not DashboardPageViewModel viewModel)
                    return;

                viewModel.ShowAddAccountFormRequested += ShowAddAccountForm;
                viewModel.ShowAccountOptionsFormRequested += ShowAccountOptionsForm;

                BindingContext = viewModel;

                await viewModel.LoadAccountsAsync();

                foreach (BankAccountViewModel account in viewModel.BankAccounts)
                    account.ShowTransactions = false;

                isInitialized = true;

                Console.WriteLine("DashboardPageViewModel initialized successfully.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing DashboardPageViewModel: {ex.Message}\n");
            }
            finally
            {
                LoadingOverlay.IsVisible = false;
                MainContent.IsVisible = true;
            }
        }

        // Override the OnAppearing method to initialize the view model when the page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Console.WriteLine("Dashboard appearing...");

            if (!isInitialized)
                await InitializeViewModelAsync();
        }

        // Show the Add Account form
        private async Task ShowAddAccountForm(AddAccountViewModel viewModel)
        {
            try
            {
                Console.WriteLine("Opening Add Account form...");

                viewModel.Reset();

                AddAccountView page = new()
                {
                    BindingContext = viewModel
                };

                await Navigation.PushModalAsync(page);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening Add Account form: {ex.Message}\n");
            }
        }

        // Show the Account Options form for a specific account
        private async Task ShowAccountOptionsForm(AccountOptionsViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Opening account options for account: {viewModel?.SelectedAccount?.Name} (ID: {viewModel?.SelectedAccount?.Model.Id})");

                AccountOptionsView accountOptionsPopup = new()
                {
                    BindingContext = viewModel
                };

                await this.ShowPopupAsync(
                    accountOptionsPopup,
                    new PopupOptions
                    {
                        Shape = null,
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening account options: {ex.Message}\n");
            }
        }
    }
}