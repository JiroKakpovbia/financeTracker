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

        private List<TransactionViewModel> Transactions { get; } = [];

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
                Transactions.Clear();
                FilteredTransactions.Clear();

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

                Transactions.Sort((a, b) => b.Date.CompareTo(a.Date));

                ApplySearch();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}\n");
            }
        }

        private void ApplySearch()
        {
            string query = SearchQuery.Trim();

            IEnumerable<TransactionViewModel> transactions =
                Transactions;

            if (!string.IsNullOrWhiteSpace(query))
            {
                transactions = Transactions.Where(transaction =>
                    transaction.Description?.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase) == true
                ||
                transaction.AccountName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase)
                ||
                transaction.AccountInstitution.ToString().Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase)
                ||
                transaction.SubCategory?.Name.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase) == true
                ||
                transaction.Amount.ToString().Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase)
                );
            }

            FilteredTransactions.Clear();

            foreach (TransactionViewModel transaction in transactions)
                FilteredTransactions.Add(transaction);

            SearchResults = FilteredTransactions.Count;
        }

        private CancellationTokenSource? searchCancellationTokenSource;

        partial void OnSearchQueryChanged(string value)
        {
            _ = DebounceSearchAsync();
        }

        private async Task DebounceSearchAsync()
        {
            searchCancellationTokenSource?.Cancel();
            searchCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, searchCancellationTokenSource.Token);

                ApplySearch();
            }
            catch (TaskCanceledException)
            {
                // User typed another character before the delay finished.
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
