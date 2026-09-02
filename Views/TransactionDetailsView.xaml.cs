using CommunityToolkit.Maui.Views;
using trackr.ViewModels;

namespace trackr.Views
{
    public partial class TransactionDetailsView : Popup
    {
        private TransactionDetailsViewModel? viewModel;

        // Constructor for TransactionDetailsView
        public TransactionDetailsView()
        {
            InitializeComponent();

            BindingContextChanged += OnBindingContextChanged;

            // Set the width of the popup to 80% of the screen width
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            var screenWidth = displayInfo.Width / displayInfo.Density;

            PopupBorder.WidthRequest = screenWidth * 0.8;
        }

        // Handle the change in BindingContext to manage the view model
        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            if (viewModel is not null)
            {
                viewModel.CloseRequested -= OnCloseRequested;
                viewModel.EditCategoryRequested -= OnEditCategoryRequested;
            }

            viewModel = BindingContext as TransactionDetailsViewModel;

            if (viewModel is not null)
            {
                viewModel.CloseRequested += OnCloseRequested;
                viewModel.EditCategoryRequested += OnEditCategoryRequested;
            }
        }

        // Handle the request to edit the category of the selected transaction
        private async Task OnEditCategoryRequested(
    TransactionViewModel transaction)
        {
            IServiceProvider? services =
        Application.Current?.Handler?.MauiContext?.Services;

            if (services?.GetService(typeof(CategorySelectorViewModel))
                is not CategorySelectorViewModel categorySelectorViewModel)
            {
                Console.WriteLine(
                    "Unable to resolve CategorySelectorViewModel.");

                return;
            }

            await categorySelectorViewModel.InitializeAsync(transaction);

            CategorySelectorView page = new()
            {
                BindingContext = categorySelectorViewModel
            };


            await Application.Current!.Windows[0].Page!
                .Navigation
                .PushModalAsync(page);
        }

        // Handle the CloseRequested event from the view model
        private async Task OnCloseRequested()
        {
            viewModel?.CloseRequested -= OnCloseRequested;
            viewModel?.EditCategoryRequested -= OnEditCategoryRequested;

            await CloseAsync();

            Console.WriteLine("Transaction Details view closed.");
        }
    }
}
