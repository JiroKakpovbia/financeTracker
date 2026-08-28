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
            viewModel?.CloseRequested -= OnCloseRequested;

            viewModel = BindingContext as TransactionDetailsViewModel;

            viewModel?.CloseRequested += OnCloseRequested;
        }

        // Handle the CloseRequested event from the view model
        private async Task OnCloseRequested()
        {
            viewModel?.CloseRequested -= OnCloseRequested;

            await CloseAsync();

            Console.WriteLine("Transaction Details view closed.");
        }
    }
}
