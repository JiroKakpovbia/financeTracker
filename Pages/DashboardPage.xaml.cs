using CommunityToolkit.Maui.Extensions;
using trackr.Views;
using trackr.ViewModels;
using CommunityToolkit.Maui;

namespace trackr.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private bool viewModelInitialized;

        // Constructor for DashboardPage
        public DashboardPage()
        {
            InitializeComponent();
        }

        // Initialize the DashboardViewModel and set up event handlers
        private async void InitializeViewModel()
        {
            Console.WriteLine("Initializing DashboardViewModel...");
            try
            {
                if (!viewModelInitialized)
                {
                    LoadingOverlay.IsVisible = true;
                    MainContent.IsVisible = false;

                    IServiceProvider? services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
                    
                    if (services?.GetService(typeof(DashboardViewModel)) is not DashboardViewModel viewModel)
                        return;

                    viewModel.ShowAddAccountFormRequested += ShowAddAccountForm;
                    viewModel.ShowAccountOptionsFormRequested += ShowAccountOptionsForm;

                    BindingContext = viewModel;

                    if (BindingContext is DashboardViewModel model)
                    {
                        await model.LoadAccountsAsync();

                        foreach (BankAccountViewModel account in model.BankAccounts)
                            account.ShowTransactions = false;
                    }

                    viewModelInitialized = true;

                    Console.WriteLine("DashboardViewModel initialized successfully.\n");
                }
                else
                {
                    Console.WriteLine("DashboardViewModel is already initialized. Skipping initialization.\n");
                }

                LoadingOverlay.IsVisible = false;
                MainContent.IsVisible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing DashboardViewModel: {ex.Message}\n");
                await DisplayAlertAsync("Error", "An unexpected error occurred while initializing the dashboard.", "OK");
            }
        }

        // Override the OnAppearing method to initialize the view model when the page appears
        protected override async void OnAppearing()
        {
            Console.WriteLine("DashboardPage appearing...");
            try
            {
                base.OnAppearing();
                InitializeViewModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnAppearing: {ex.Message}\n");
                await DisplayAlertAsync("Error", "An unexpected error occurred while loading the dashboard.", "OK");
            }
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
                Console.WriteLine($"Opening account options for account: {viewModel?.SelectedAccount?.Name} (ID: {viewModel?.SelectedAccount?.Id})");

                AccountOptionsView accountOptionsPopup = new()
                {
                    BindingContext = viewModel
                };

                await this.ShowPopupAsync(
                    accountOptionsPopup,
                    new PopupOptions
                    {
                        Shape = null,
                        Shadow = null
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling menu action: {ex.Message}\n");
            }
        }
    }
}