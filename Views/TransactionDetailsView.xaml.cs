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
