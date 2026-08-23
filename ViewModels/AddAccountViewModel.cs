using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class AddAccountViewModel : ObservableObject
    {

        public class PendingCSVImport
        {
            public string FileName { get; set; } = string.Empty;

            public ObservableCollection<Transaction> Transactions { get; set; } = [];

            public int PossibleDuplicateCount { get; set; }

            public int ErrorCount { get; set; }
        }


        [ObservableProperty]
        private string accountName = string.Empty;

        [ObservableProperty]
        private BankInstitution? selectedInstitution;

        [ObservableProperty]
        private AccountType? selectedType;

        [ObservableProperty]
        private decimal? currentBalance = null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPendingImport))]
        [NotifyPropertyChangedFor(nameof(ShowImportCSVButton))]
        private PendingCSVImport? pendingImport;

        public bool HasPendingImport => PendingImport is not null;

        public bool ShowImportCSVButton => !HasPendingImport;

        public IEnumerable<BankInstitution> BankInstitutions { get; } = Enum.GetValues<BankInstitution>().ToList();

        public IEnumerable<AccountType> AccountTypes { get; } = Enum.GetValues<AccountType>().ToList();

        public event Func<AddAccountViewModel, Task>? ImportCSVRequested;

        public event Func<AddAccountViewModel, Task>? AddAccountRequested;

        [RelayCommand]
        private async Task ImportCSV()
        {
            if (ImportCSVRequested != null)
                await ImportCSVRequested.Invoke(this);
        }

        [RelayCommand]
        private async Task AddAccount()
        {
            if (AddAccountRequested != null)
                await AddAccountRequested.Invoke(this);
        }

        [RelayCommand]
        private void ClearPendingImport()
        {
            PendingImport = null;
        }

        public void Reset()
        {
            AccountName = string.Empty;
            SelectedInstitution = null;
            SelectedType = null;
            CurrentBalance = null;

            ClearPendingImport();
        }
    }
}