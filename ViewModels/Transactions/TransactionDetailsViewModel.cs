using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace trackr.ViewModels
{
    public partial class TransactionDetailsViewModel() : ObservableObject
    {
        [ObservableProperty]
        private TransactionViewModel? selectedTransaction;

        public event Func<Task>? CloseRequested;

        public event Func<TransactionViewModel, Task>? EditCategoryRequested;

        private async Task RequestClose()
        {
            if (CloseRequested is not null)
                await CloseRequested.Invoke();
        }

        // Handle the request to edit the category of the selected transaction
        [RelayCommand]
        private async Task EditCategory()
        {
            if (SelectedTransaction is null)
                return;

            if (EditCategoryRequested is not null)
                await EditCategoryRequested.Invoke(SelectedTransaction);
        }

        // Handle the cancellation of the transaction details view
        [RelayCommand]
        private async Task Close()
        {
            Console.WriteLine("Closing Transaction Details view...");

            await RequestClose();

            SelectedTransaction = null;
        }
    }
}