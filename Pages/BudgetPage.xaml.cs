using trackr.ViewModels;

namespace trackr.Pages
{
    public partial class BudgetPage : ContentPage
    {
        private bool isInitialized;

        // Constructor for BudgetPage
        public BudgetPage()
        {
            InitializeComponent();
        }

        // Initialize the BudgetPageViewModel and set up event handlers
        private async Task InitializeViewModelAsync()
        {
            Console.WriteLine("Initializing BudgetPageViewModel...");
            
            LoadingOverlay.IsVisible = true;
            MainContent.IsVisible = false;

            try
            {
                IServiceProvider? services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;

                if (services?.GetService(typeof(BudgetPageViewModel)) is not BudgetPageViewModel viewModel)
                    return;

                BindingContext = viewModel;

                await viewModel.LoadCategoriesAsync();

                isInitialized = true;

                Console.WriteLine("BudgetPageViewModel initialized successfully.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing BudgetPageViewModel: {ex.Message}\n");
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
            try
            {
                base.OnAppearing();

                Console.WriteLine("Budget page appearing...");

                if (!isInitialized)
                    await InitializeViewModelAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnAppearing: {ex.Message}\n");
            }
        }

    }
}