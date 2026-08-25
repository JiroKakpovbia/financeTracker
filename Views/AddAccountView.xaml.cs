using trackr.ViewModels;

namespace trackr.Views
{
    public partial class AddAccountView : ContentPage
    {
        private AddAccountViewModel? viewModel;

        // Constructor for AddAccountView
        public AddAccountView()
        {
            InitializeComponent();

            BindingContextChanged += OnBindingContextChanged;
        }

        // Handle the change in BindingContext to manage the view model
        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            viewModel?.CloseRequested -= OnCloseRequested;

            viewModel = BindingContext as AddAccountViewModel;

            viewModel?.CloseRequested += OnCloseRequested;
        }

        // Handle the request to close the AddAccountView from the ViewModel
        private async Task OnCloseRequested()
        {
            await Navigation.PopModalAsync();

            Console.WriteLine("Add Account form closed.");
        }
    }
}
