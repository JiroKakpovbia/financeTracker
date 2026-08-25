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

        // Handle the change in BindingContext to manage event subscriptions
        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.ShowAlertRequested -= OnShowAlertRequested;
                viewModel.CloseRequested -= OnCloseRequested;
            }

            viewModel = BindingContext as AddAccountViewModel;

            if (viewModel != null)
            {
                viewModel.ShowAlertRequested += OnShowAlertRequested;
                viewModel.CloseRequested += OnCloseRequested;
            }
        }

        // Handle the request to show an alert from the ViewModel
        private async Task OnShowAlertRequested(
            string title,
            string message)
        {
            await DisplayAlertAsync(
                title,
                message,
                "OK");
        }

        // Handle the request to close the AddAccountView from the ViewModel
        private async Task OnCloseRequested()
        {
            await Navigation.PopModalAsync();
        }
    }
}
