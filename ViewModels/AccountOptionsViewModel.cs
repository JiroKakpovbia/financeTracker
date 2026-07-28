using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace trackr.ViewModels
{
    public partial class AccountOptionsViewModel : ObservableObject
    {
        [ObservableProperty]
        private BankAccountViewModel? selectedAccount;
        
        public event Func<AccountOptionsViewModel, Task>? RenameAccountRequested;
        public event Func<AccountOptionsViewModel, Task>? ImportCSVRequested;
        public event Func<AccountOptionsViewModel, Task>? MoveAccountRequested;
        public event Func<AccountOptionsViewModel, Task>? DeleteAccountRequested;

        [RelayCommand]
        private async Task RenameAccountAsync()
        {
            if (RenameAccountRequested != null)
                await RenameAccountRequested.Invoke(this);
        }

        [RelayCommand]
        private async Task ImportCSVAsync()
        {
            if (ImportCSVRequested != null)
                await ImportCSVRequested.Invoke(this);
        }

        [RelayCommand]
        private async Task MoveAccountAsync()
        {
            if (MoveAccountRequested != null)
                await MoveAccountRequested.Invoke(this);
        }

        [RelayCommand]
        private async Task DeleteAccountAsync()
        {
            if (DeleteAccountRequested != null)
                await DeleteAccountRequested.Invoke(this);
        }
    }
}