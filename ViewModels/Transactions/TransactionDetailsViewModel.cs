using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Factories;
using trackr.Import;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class TransactionDetailsViewModel(IDialogService dialogService, IAccountDataService accountDataService) : ObservableObject
    {
        [ObservableProperty]
        private TransactionViewModel? selectedTransaction;

        public event Func<Task>? CloseRequested;

        public event Func<Task>? CategoryChanged;

        private async Task RequestClose()
        {
            if (CloseRequested is not null)
                await CloseRequested.Invoke();
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