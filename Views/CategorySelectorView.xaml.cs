using trackr.ViewModels;

namespace trackr.Views
{
    public partial class CategorySelectorView : ContentPage
    {
        private CategorySelectorViewModel? viewModel;

        // Constructor for CategorySelectorView
        public CategorySelectorView()
        {
            InitializeComponent();

            BindingContextChanged += OnBindingContextChanged;
        }

        // Handle the change in BindingContext to manage the view model
        private void OnBindingContextChanged(object? sender, EventArgs e)
        {
            viewModel?.CloseRequested -= OnCloseRequested;

            viewModel = BindingContext as CategorySelectorViewModel;

            viewModel?.CloseRequested += OnCloseRequested;
        }

        // Handle the request to close the CategorySelectorView from the ViewModel
        private async Task OnCloseRequested()
        {
            await Navigation.PopModalAsync();

            Console.WriteLine("Category Selector form closed.");
        }
    }
}
