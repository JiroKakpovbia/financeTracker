using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using trackr.Factories;
using trackr.Messages;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class SearchPageViewModel : ObservableObject
    {
        private readonly IAccountDataService accountDataService;
        private readonly ITransactionViewModelFactory transactionViewModelFactory;

        public TransactionDetailsViewModel TransactionDetailsViewModel { get; }

        private readonly List<TransactionViewModel> allTransactions = []; // This list will hold all transactions loaded from the database, regardless of the search query

        private List<TransactionViewModel> filteredTransactions = []; // This list will hold the transactions that match the current search query

        public ObservableCollection<TransactionViewModel> DisplayedTransactions { get; } = []; // This collection will hold the transactions that are currently displayed in the UI, based on the current page of filtered transactions

        [ObservableProperty]
        private int searchResults;

        [ObservableProperty]
        public bool noResultsFound;

        [ObservableProperty]
        public bool hasResults;

        [ObservableProperty]
        private string searchQuery = string.Empty;

        private CancellationTokenSource? searchCancellationTokenSource;

        private const int PageSize = 30;

        [RelayCommand]
        private void LoadMore()
        {
            LoadNextPage();
        }

        // Load transactions from the database and populate the Transaction lists
        public async Task LoadTransactionsAsync()
        {
            try
            {
                // Clear the existing transactions and filtered transactions
                allTransactions.Clear();
                filteredTransactions.Clear();
                DisplayedTransactions.Clear();

                IReadOnlyList<Transaction> transactions = await accountDataService.GetAllTransactionsAsync();

                if (transactions is null || transactions.Count == 0)
                {
                    Console.WriteLine("No transactions found.\n");

                    SearchResults = 0;
                    NoResultsFound = true;
                    HasResults = false;

                    return;
                }

                // Create TransactionViewModel instances for each transaction and add them to allTransactions
                foreach (Transaction transaction in transactions)
                {
                    TransactionViewModel transactionViewModel =
                        await transactionViewModelFactory.CreateAsync(transaction);

                    allTransactions.Add(transactionViewModel);
                }

                // Apply the search filter to the transactions
                ApplySearch();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}\n");
            }
        }

        // Apply the search filter to the transactions based on the current search query
        private void ApplySearch()
        {
            string query = SearchQuery.Trim();

            // If the search query is empty, display all transactions
            if (string.IsNullOrWhiteSpace(query))
            {
                filteredTransactions = allTransactions;
            }
            // If the search query is not empty, filter the transactions based on the search query
            else
            {
                filteredTransactions =
                [
                    .. allTransactions.Where(transaction =>
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
                )];
            }

            // Sort the filtered transactions by date in descending order
            filteredTransactions.Sort((a, b) => b.Date.CompareTo(a.Date));

            // Reset the displayed transactions count and clear the FilteredTransactions collection
            DisplayedTransactions.Clear();

            SearchResults = filteredTransactions.Count;

            // Load the next page of filtered transactions and update the search results count
            LoadNextPage();

            HasResults = DisplayedTransactions.Count > 0;

            NoResultsFound =
                DisplayedTransactions.Count == 0 &&
                !string.IsNullOrWhiteSpace(SearchQuery);
        }

        // Called whenever the SearchQuery property changes
        partial void OnSearchQueryChanged(string value)
        {
            _ = DebounceSearchAsync();
        }

        // Debounce the search input to avoid excessive filtering while the user is typing
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

        // Load the next page of filtered transactions and add them to the FilteredTransactions collection
        private void LoadNextPage()
        {
            // Get the next page of filtered transactions based on the current displayed transactions count and the page size
            IEnumerable<TransactionViewModel> nextPage =
                filteredTransactions
                    .Skip(DisplayedTransactions.Count)
                    .Take(PageSize);

            // Add the next page of filtered transactions to the FilteredTransactions collection and update the displayed transactions count
            foreach (TransactionViewModel transaction in nextPage)
                DisplayedTransactions.Add(transaction);
        }

        // Handle the addition of a new account by loading its transactions and updating the displayed transactions in the search results
        public async Task OnAccountAddedAsync(Guid accountId)
        {
            Console.WriteLine(
                $"SearchPage loading transactions for new account {accountId}.");

            IReadOnlyList<Transaction> transactions =
                await accountDataService.GetTransactionsForAccountAsync(accountId);

            foreach (Transaction transaction in transactions)
            {
                TransactionViewModel transactionViewModel =
                    await transactionViewModelFactory.CreateAsync(transaction);

                allTransactions.Add(transactionViewModel);
            }

            // Re-apply the search filter to update the displayed transactions
            ApplySearch();

            Console.WriteLine(
                $"SearchPage loaded transactions for new account {accountId}.");
        }

        // Handle the update of an account by refreshing the transactions and their display in the search results
        private async Task OnAccountUpdatedAsync(Guid accountId)
        {
            Console.WriteLine(
                $"SearchPage updating transactions for updated account {accountId}.");

            IReadOnlyList<Transaction> transactions =
                await accountDataService.GetTransactionsForAccountAsync(accountId);

            allTransactions.RemoveAll(t => t.Model.BankAccountId == accountId);

            // Add the updated transactions for the account to the allTransactions list
            foreach (Transaction transaction in transactions)
            {
                TransactionViewModel transactionViewModel =
                    await transactionViewModelFactory
                        .CreateAsync(transaction);

                allTransactions.Add(transactionViewModel);
            }

            // Re-apply the search filter to update the displayed transactions
            ApplySearch();

            Console.WriteLine(
                $"SearchPage updated transactions for account {accountId}.");
        }

        // Handle the deletion of an account by removing its associated transactions from the allTransactions list and updating the displayed transactions
        private async Task OnAccountDeletedAsync(Guid accountId)
        {
            Console.WriteLine(
                $"SearchPage removing transactions for deleted account {accountId}.");

            // Remove transactions associated with the deleted account from the allTransactions list
            allTransactions.RemoveAll(t => t.Model.BankAccountId == accountId);

            // Re-apply the search filter to update the displayed transactions
            ApplySearch();

            Console.WriteLine(
                $"SearchPage removed transactions for deleted account {accountId}.");
        }

        // Handle the update of a transaction by refreshing the corresponding TransactionViewModel in the allTransactions list and updating the displayed transactions
        private async Task OnTransactionUpdatedAsync(int transactionId)
        {
            Console.WriteLine(
                $"SearchPage updating transaction {transactionId}.");

            Transaction? updatedTransaction =
                await accountDataService.GetTransactionAsync(transactionId);

            if (updatedTransaction is null)
                return;

            TransactionViewModel existingTransaction =
                allTransactions.First(
                    t => t.Model.Id == transactionId);

            int index =
                allTransactions.IndexOf(existingTransaction);

            TransactionViewModel updatedTransactionViewModel =
                await transactionViewModelFactory.CreateAsync(
                    updatedTransaction);

            allTransactions[index] = updatedTransactionViewModel;

            // Re-apply the search filter to update the displayed transactions
            ApplySearch();

            Console.WriteLine(
                $"SearchPage updated transaction {transactionId}.");

        }

        // Constructor for SearchPageViewModel
        public SearchPageViewModel(IAccountDataService accountDataService, TransactionDetailsViewModel transactionDetailsViewModel, ITransactionViewModelFactory transactionViewModelFactory)
        {
            this.accountDataService = accountDataService;

            this.transactionViewModelFactory = transactionViewModelFactory;

            TransactionDetailsViewModel = transactionDetailsViewModel;

            WeakReferenceMessenger.Default.Register<
                SearchPageViewModel,
                AccountAddedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnAccountAddedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                SearchPageViewModel,
                AccountUpdatedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnAccountUpdatedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                SearchPageViewModel,
                AccountDeletedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnAccountDeletedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                SearchPageViewModel,
                TransactionUpdatedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnTransactionUpdatedAsync(
                        message.Value);
                });
        }
    }
}
