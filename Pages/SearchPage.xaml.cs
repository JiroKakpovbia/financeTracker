using CommunityToolkit.Maui.Extensions;
using trackr.Views;
using trackr.ViewModels;
using CommunityToolkit.Maui;

namespace trackr.Pages
{
    public partial class SearchPage : ContentPage
    {
        private bool isInitialized;


        // Constructor for SearchPage
        public SearchPage()
        {
            InitializeComponent();
        }

        // Initialize the SearchPageViewModel and set up event handlers
        private async Task InitializeViewModelAsync()
        {
            Console.WriteLine("Initializing SearchPageViewModel...");
            LoadingOverlay.IsVisible = true;
            MainContent.IsVisible = false;

            try
            {
                IServiceProvider? services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;

                if (services?.GetService(typeof(SearchPageViewModel)) is not SearchPageViewModel viewModel)
                    return;

                BindingContext = viewModel;

                await viewModel.LoadTransactionsAsync();

                isInitialized = true;

                Console.WriteLine("SearchPageViewModel initialized successfully.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SearchPageViewModel: {ex.Message}\n");
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

                Console.WriteLine("Search page appearing...");

                if (!isInitialized)
                    await InitializeViewModelAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnAppearing: {ex.Message}\n");
            }
        }

        private async void OnTransactionSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not TransactionViewModel transaction)
                return;

            if (BindingContext is not SearchPageViewModel viewModel)
                return;

            // Give TransactionDetailsViewModel the clicked transaction
            viewModel.TransactionDetailsViewModel.SelectedTransaction = transaction;

            Console.WriteLine($"Transaction selected: {transaction.Description} ({transaction.Amount:C})");

            // Create the popup and give it its ViewModel
            TransactionDetailsView popup = new()
            {
                BindingContext = viewModel.TransactionDetailsViewModel
            };

            // Clear selection so the same transaction can be clicked again later.
            if (sender is CollectionView collectionView)
                collectionView.SelectedItem = null;

            await this.ShowPopupAsync(popup);
        }
    }
}