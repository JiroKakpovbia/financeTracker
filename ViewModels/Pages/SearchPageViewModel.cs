using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using trackr.Factories;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class SearchPageViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly IAccountDataService accountDataService;
        private readonly IBankAccountViewModelFactory bankAccountViewModelFactory;

        public ObservableCollection<TransactionViewModel> Transactions { get; } = [];

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private int searchResults;

        // Load transactions from the database and populate the Transactions collection
        public async Task LoadTransactionsAsync()
        {
            try
            {
                Transactions.Clear();

                IReadOnlyList<Transaction> transactions = await accountDataService.LoadTransactionsAsync(null);

                IReadOnlyList<BankAccount> accounts = await accountDataService.LoadAccountsAsync();

                // Create TransactionViewModel instances for each transaction through the BankAccountViewModel and add them to the Transactions collection
                foreach (BankAccount account in accounts)
                {
                    // This will also create TransactionViewModel instances for each transaction associated with the account
                    BankAccountViewModel accountViewModel =
                        await bankAccountViewModelFactory.CreateAsync(account);

                    // Add the transactions from the accountViewModel to the Transactions collection
                    foreach (TransactionViewModel transaction in accountViewModel.Transactions)
                        Transactions.Add(transaction);
                }

                SearchResults = Transactions.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}\n");
            }
        }


        // Constructor for SearchPageViewModel
        public SearchPageViewModel(IDialogService dialogService, IAccountDataService accountDataService, IBankAccountViewModelFactory bankAccountViewModelFactory)
        {
            this.dialogService = dialogService;
            this.accountDataService = accountDataService;
            this.bankAccountViewModelFactory = bankAccountViewModelFactory;
        }
    }
}
