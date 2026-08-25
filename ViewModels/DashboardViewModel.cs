using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly IAccountDataService accountDataService;

        public AddAccountViewModel AddAccountViewModel { get; }
        public AccountOptionsViewModel AccountOptionsViewModel { get; }

        [ObservableProperty]
        private ObservableCollection<BankAccountViewModel> bankAccounts = [];

        [ObservableProperty]
        private decimal netWorth;

        [ObservableProperty]
        private decimal assets;

        [ObservableProperty]
        private decimal liabilities;

        // Events for UI interactions
        public event Func<AddAccountViewModel, Task>? ShowAddAccountFormRequested;

        public event Func<AccountOptionsViewModel, Task>? ShowAccountOptionsFormRequested;

        // Request to show the Add Account form
        private async Task RequestShowAddAccountForm()
        {
            if (ShowAddAccountFormRequested == null)
                return;

            await ShowAddAccountFormRequested.Invoke(AddAccountViewModel);
        }

        // Request to show the Account Options form for a specific account
        private async Task RequestShowAccountOptionsForm(BankAccountViewModel? account)
        {
            AccountOptionsViewModel.SelectedAccount = account;

            if (ShowAccountOptionsFormRequested == null)
                return;

            await ShowAccountOptionsFormRequested.Invoke(AccountOptionsViewModel);
        }

        [RelayCommand]
        private Task ShowAddAccountFormAsync() => RequestShowAddAccountForm();

        [RelayCommand]
        private Task ShowAccountOptionsAsync(BankAccountViewModel? account) => RequestShowAccountOptionsForm(account);

        [RelayCommand]
        private Task ToggleTransactionsAsync(BankAccountViewModel? account) => HandleToggleTransactions(account);

        [RelayCommand]
        private Task LogoTapAsync(BankAccountViewModel? account) => HandleLogoTap(account);

        // Load accounts from the database and populate the BankAccounts collection
        public async Task LoadAccountsAsync()
        {
            try
            {
                ObservableCollection<BankAccount> accounts = await accountDataService.LoadAccountsAsync();

                BankAccounts = new ObservableCollection<BankAccountViewModel>(
                    accounts.Select(a => new BankAccountViewModel(a))
                );

                foreach (BankAccountViewModel account in BankAccounts)
                {
                    ObservableCollection<Transaction> transactions = await accountDataService.LoadTransactionsAsync(account.Model);
                    ObservableCollection<ImportBatch> importBatches = await accountDataService.LoadImportBatchesAsync(account.Model);

                    account.Transactions = new ObservableCollection<TransactionViewModel>(transactions.Select(t => new TransactionViewModel(t)));
                    account.ImportBatches = new ObservableCollection<ImportBatchViewModel>(importBatches.Select(b => new ImportBatchViewModel(b)));

                    // Set the import batch list and last import batch for the account if there are any import batches
                    if (importBatches.Count > 0)
                    {
                        account.ImportBatches = new ObservableCollection<ImportBatchViewModel>(importBatches.Select(b => new ImportBatchViewModel(b)));
                        account.LastImport = account.ImportBatches.OrderByDescending(b => b.ImportedAt).FirstOrDefault();
                    }

                    // Set the import batch for each transaction based on the ImportBatchId
                    foreach (TransactionViewModel transaction in account.Transactions)
                    {
                        ImportBatchViewModel? importBatch = account.ImportBatches.FirstOrDefault(b => b.Id == transaction.Model.ImportBatchId);
                        transaction.ImportedAt = importBatch?.ImportedAt ?? DateTime.MinValue;
                    }

                    // Refresh the transaction groups for the account to ensure they are grouped and ordered correctly for the UI
                    account.RefreshTransactionGroups();
                }

                await UpdateNetWorthTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "Failed to load accounts. Please try again.");
            }
        }

        // Handle the addition of a new account by adding it to the BankAccounts collection and updating net worth totals
        private async Task OnAccountAdded(BankAccountViewModel account)
        {
            BankAccounts.Add(account);

            await UpdateNetWorthTotals();
        }

        // Handle the deletion of an account by removing it from the BankAccounts collection and updating net worth totals
        private async Task OnAccountDeleted(BankAccountViewModel account)
        {
            BankAccounts.Remove(account);

            await UpdateNetWorthTotals();
        }

        // Update the net worth, assets, and liabilities totals based on the current accounts
        private async Task UpdateNetWorthTotals()
        {
            NetWorth = BankAccounts.Sum(account => account.ReconciledBalance);
            Assets = BankAccounts
                .Where(account => account.ReconciledBalance > 0)
                .Sum(account => account.ReconciledBalance);
            Liabilities = BankAccounts
                .Where(account => account.ReconciledBalance < 0)
                .Sum(account => account.ReconciledBalance);

            await Task.CompletedTask;
        }

        // Handle toggling the visibility of transactions for a specific account
        private async Task HandleToggleTransactions(BankAccountViewModel? account)
        {
            try
            {
                if (account == null)
                    return;

                // If there are no transactions, show an alert to the user
                if (account.TransactionGroups == null || account.TransactionGroups.Count == 0)
                {
                    await dialogService.ShowAlertAsync(
                        "No Transactions",
                        "This account has no transactions to show. Import a CSV to populate this account.");

                    return;
                }

                account.ShowTransactions = !account.ShowTransactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling transactions: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while toggling transactions.");
            }
        }

        // Handle the tap on the bank logo to open the bank's app or website
        private async Task HandleLogoTap(BankAccountViewModel? account)
        {
            Console.WriteLine($"Handling logo tap for account: {account?.Name} (ID: {account?.Id})");
            try
            {
                Uri? appUri = null;
                Uri? webUri = null;

                if (account == null)
                    return;

                if (account.Institution == BankInstitution.TD)
                {
                    appUri = new Uri("td://");
                    webUri = new Uri("https://easyweb.td.com/ui/ew/fs?fsType=PFS");
                }
                else if (account.Institution == BankInstitution.CIBC)
                {
                    appUri = new Uri("cibc://");
                    webUri = new Uri("https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html#/accounts/credit-cards/2c01046615744246b6ecadead422be4ddefd7b72ac9a7f7912f70bb70ab89bbe");
                }
                else if (account.Institution == BankInstitution.CapitalOne)
                {
                    appUri = new Uri("capitalone://");
                    webUri = new Uri("https://myaccounts.capitalone.com/accountSummary");
                }
                else if (account.Institution == BankInstitution.RBC)
                {
                    appUri = new Uri("rbc://");
                    webUri = new Uri("https://www1.royalbank.com/sgw1/olb/index-en/#/summary");
                }
                else
                {
                    await dialogService.ShowAlertAsync(
                    "Error",
                    "Failed to open bank's website or application.");
                }

                if (appUri != null)
                {
                    bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

                    if (canOpen)
                        await Launcher.Default.OpenAsync(appUri);
                    else
                        await dialogService.ShowAlertAsync(
                            "Error",
                            "Failed to open bank's application.");
                }

                if (webUri != null)
                {
                    bool canOpen = await Launcher.Default.CanOpenAsync(webUri);

                    if (canOpen)
                        await Launcher.Default.OpenAsync(webUri);
                    else
                        await dialogService.ShowAlertAsync(
                            "Error",
                            "Failed to open bank's website.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening bank's website or application: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    $"An unexpected error occurred: {ex.Message}");
            }
        }

        // Constructor for DashboardViewModel
        public DashboardViewModel(IDialogService dialogService, IAccountDataService accountDataService, AddAccountViewModel addAccountViewModel, AccountOptionsViewModel accountOptionsViewModel)
        {
            this.dialogService = dialogService;
            this.accountDataService = accountDataService;

            AddAccountViewModel = addAccountViewModel;
            AccountOptionsViewModel = accountOptionsViewModel;

            AddAccountViewModel.AccountAdded += OnAccountAdded;

            AccountOptionsViewModel.AccountDeleted += OnAccountDeleted;
            AccountOptionsViewModel.AccountBalanceChanged += UpdateNetWorthTotals;
        }
    }
}
