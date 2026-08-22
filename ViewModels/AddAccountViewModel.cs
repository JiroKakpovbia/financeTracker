using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class AddAccountViewModel : ObservableObject
    {
        [ObservableProperty]
        private string accountName = "";

        [ObservableProperty]
        private BankInstitution? selectedInstitution;

        [ObservableProperty]
        private AccountType? selectedType;

        [ObservableProperty]
        private decimal currentBalance;

        public IEnumerable<BankInstitution> BankInstitutions { get; } = Enum.GetValues<BankInstitution>().ToList();
        public IEnumerable<AccountType> AccountTypes { get; } = Enum.GetValues<AccountType>().ToList();

        public event Func<AddAccountViewModel, Task>? AddAccountRequested;

        [RelayCommand]
        private async Task AddAccount()
        {
            if (AddAccountRequested != null)
                await AddAccountRequested.Invoke(this);
        }

        public void Reset()
        {
            AccountName = string.Empty;
            SelectedInstitution = null;
            SelectedType = null;
            CurrentBalance = 0m;
        }
    }
}