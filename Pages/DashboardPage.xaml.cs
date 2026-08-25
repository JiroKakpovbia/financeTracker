using CommunityToolkit.Maui.Extensions;
using trackr.Views;
using trackr.ViewModels;
using CommunityToolkit.Maui;

namespace trackr.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private bool viewModelInitialized;
        private AccountOptionsView? accountOptionsPopup;

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

                    viewModel.ShowAlertRequested += OnShowAlertRequested;
                    viewModel.ShowPromptRequested += OnShowPromptRequested;

                    viewModel.ShowAddAccountFormRequested += ShowAddAccountForm;

                    viewModel.ShowAccountOptionsFormRequested += ShowAccountOptionsForm;
                    viewModel.HideAccountOptionsFormRequested += HideAccountOptionsForm;

                    BindingContext = viewModel;

                    if (BindingContext is DashboardViewModel model)
                    {
                        await model.LoadAccountsAsync();

                        foreach (BankAccountViewModel account in model.BankAccounts)
                        {
                            account.ShowTransactions = false;
                        }
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

        // Handle the ShowAlertRequested event from the DashboardViewModel
        private async Task<bool> OnShowAlertRequested(object? sender, DashboardViewModel.AlertEventArgs args)
        {
            try
            {
                if (args.Title.Contains("Confirm")) return await DisplayAlertAsync(args.Title, args.Message, "Yes", "Cancel");
                else
                {
                    await DisplayAlertAsync(args.Title, args.Message, "OK");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing alert: {ex.Message}\n");
                await DisplayAlertAsync("Error", "An unexpected error occurred while displaying an alert.", "OK");
                return false;
            }
        }

        // Handle the ShowPromptRequested event from the DashboardViewModel
        private async Task<string?> OnShowPromptRequested(object? sender, DashboardViewModel.PromptEventArgs args)
        {
            try
            {
                return await DisplayPromptAsync(args.Title, args.Message, "OK", "Cancel", initialValue: args.InitialValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing prompt: {ex.Message}\n");
                await DisplayAlertAsync("Error", "An unexpected error occurred while displaying a prompt.", "OK");
                return null;
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

                accountOptionsPopup = new AccountOptionsView
                {
                    BindingContext = viewModel
                };

                await this.ShowPopupAsync(accountOptionsPopup,
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

        // Hide the Account Options form
        private async Task HideAccountOptionsForm(AccountOptionsViewModel viewModel)
        {
            try
            {
                Console.WriteLine("Hiding Account Options form...");

                // Close the Account Options popup if it's open
                if (accountOptionsPopup != null)
                    await accountOptionsPopup.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding Account Options form: {ex.Message}\n");
            }
            finally
            {
                // Ensure the popup reference is cleared even if an error occurs
                accountOptionsPopup = null;
            }
        }
    }
}