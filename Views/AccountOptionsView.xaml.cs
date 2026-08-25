using CommunityToolkit.Maui.Views;
using trackr.ViewModels;

namespace trackr.Views
{
    public partial class AccountOptionsView : Popup
    {
        private AccountOptionsViewModel? viewModel;

        // Constructor for AccountOptionsView
        public AccountOptionsView()
        {
            InitializeComponent();

            BindingContextChanged += OnBindingContextChanged;
        }

        // Handle the BindingContextChanged event to manage the view model
        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            // Remove the handler from any previous VM
            viewModel?.CloseRequested -= OnCloseRequested;

            viewModel = BindingContext as AccountOptionsViewModel;

            // Listen only for popup-close requests
            viewModel?.CloseRequested += OnCloseRequested;
        }

        // Handle the CloseRequested event from the view model
        private async Task OnCloseRequested()
        {
            viewModel?.CloseRequested -= OnCloseRequested;

            await CloseAsync();

            Console.WriteLine("Account Options form closed.");
        }
    }
}
