using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class SearchPageViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly IAccountDataService accountDataService;

        public List<TransactionViewModel> Transactions { get; } = [];
        public ObservableCollection<TransactionViewModel> FilteredTransactions { get; } = [];

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private int searchResults;

        // Load transactions from the database and populate the Transactions collection
        public async Task LoadTransactionsAsync()
        {
            try
            {
                FilteredTransactions.Clear();
                Transactions.Clear();

                IReadOnlyList<Transaction> transactions = await accountDataService.LoadTransactionsAsync(null);

                IReadOnlyList<BankAccount> accounts = await accountDataService.LoadAccountsAsync();

                List<TransactionViewModel> transactionViewModels = [.. transactions
                        .Select(t => new TransactionViewModel(t){
                            AccountName = accounts.FirstOrDefault(a => a.Id == t.BankAccountId).Name,
                            AccountInstitution = accounts.FirstOrDefault(a => a.Id == t.BankAccountId).Institution,
                        })];

                // Add each transaction view model to the Transactions collection
                foreach (TransactionViewModel transaction in transactionViewModels)
                {
                    Transactions.Add(transaction);
                    FilteredTransactions.Add(transaction);
                }
                SearchResults = FilteredTransactions.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}\n");
            }
        }


        // Constructor for SearchPageViewModel
        public SearchPageViewModel(IDialogService dialogService, IAccountDataService accountDataService)
        {
            this.dialogService = dialogService;
            this.accountDataService = accountDataService;
        }
    }
}
