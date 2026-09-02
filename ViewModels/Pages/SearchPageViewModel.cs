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
        private readonly IDialogService dialogService;
        private readonly IAccountDataService accountDataService;
        private readonly IBankAccountViewModelFactory bankAccountViewModelFactory;

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

                IReadOnlyList<BankAccount> accounts = await accountDataService.GetAllAccountsAsync();

                if (accounts == null || accounts.Count == 0)
                {
                    Console.WriteLine("No bank accounts found.\n");

                    SearchResults = 0;
                    NoResultsFound = true;
                    HasResults = false;

                    return;
                }

                // Create TransactionViewModel instances for each transaction through the BankAccountViewModel and add them to the Transactions collection
                foreach (BankAccount account in accounts)
                {
                    // This will also create TransactionViewModel instances for each transaction associated with the account
                    BankAccountViewModel accountViewModel =
                        await bankAccountViewModelFactory.CreateAsync(account);

                    // Add the transactions from the accountViewModel to the Transactions collection
                    foreach (TransactionViewModel transaction in accountViewModel.Transactions)
                        allTransactions.Add(transaction);
                }

                // Sort the transactions by date in descending order
                allTransactions.Sort((a, b) => b.Date.CompareTo(a.Date));

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

        // Handle the update of a transaction by refreshing the transactions and their display in the search results
        private async Task OnTransactionUpdatedAsync(int transactionId)
        {
            Console.WriteLine(
                $"SearchPage refreshing transaction {transactionId}.");

            await LoadTransactionsAsync();
        }

        private async Task OnTransactionsChangedAsync(Guid? accountId)
        {
            Console.WriteLine(
                $"Transactions changed for account {accountId}.");

            await LoadTransactionsAsync();
        }

        // Constructor for SearchPageViewModel
        public SearchPageViewModel(IDialogService dialogService, IAccountDataService accountDataService, TransactionDetailsViewModel transactionDetailsViewModel, IBankAccountViewModelFactory bankAccountViewModelFactory)
        {
            this.dialogService = dialogService;
            this.accountDataService = accountDataService;
            this.bankAccountViewModelFactory = bankAccountViewModelFactory;

            TransactionDetailsViewModel = transactionDetailsViewModel;

            WeakReferenceMessenger.Default.Register<
                SearchPageViewModel,
                TransactionUpdatedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnTransactionUpdatedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                SearchPageViewModel,
                TransactionsChangedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnTransactionsChangedAsync(
                        message.Value);
                });
        }
    }
}
