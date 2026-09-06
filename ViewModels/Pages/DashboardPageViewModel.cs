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
    public partial class DashboardPageViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly IAccountDataService accountDataService;

        private readonly IBankAccountViewModelFactory bankAccountViewModelFactory;
        private readonly ITransactionViewModelFactory transactionViewModelFactory;

        public AddAccountViewModel AddAccountViewModel { get; }
        public AccountOptionsViewModel AccountOptionsViewModel { get; }

        public ObservableCollection<BankAccountViewModel> BankAccounts { get; } = [];

        [ObservableProperty]
        private decimal netWorth;

        [ObservableProperty]
        private decimal assets;

        [ObservableProperty]
        private decimal liabilities;

        // Events for UI interactions
        public event Func<AddAccountViewModel, Task>? ShowAddAccountFormRequested;

        public event Func<AccountOptionsViewModel, Task>? AccountOptionsRequested;

        public event Func<BankAccountViewModel, Task>? ToggleTransactionsRequested;

        public event Func<BankAccountViewModel, Task>? LogoTapRequested;

        // Request to show the Add Account form
        private async Task RequestShowAddAccountForm()
        {
            if (ShowAddAccountFormRequested is null)
                return;

            await ShowAddAccountFormRequested.Invoke(AddAccountViewModel);
        }

        // Request to show the Account Options form for a specific account
        private async Task RequestShowAccountOptions(BankAccountViewModel account)
        {
            AccountOptionsViewModel.SelectedAccount = account;

            if (AccountOptionsRequested is null)
                return;

            await AccountOptionsRequested.Invoke(AccountOptionsViewModel);
        }

        private async Task RequestToggleTransactions(BankAccountViewModel account)
        {
            if (ToggleTransactionsRequested is null)
                return;

            await ToggleTransactionsRequested.Invoke(account);
        }

        private async Task RequestLogoTap(BankAccountViewModel account)
        {
            if (LogoTapRequested is null)
                return;

            await LogoTapRequested.Invoke(account);
        }

        [RelayCommand]
        private Task ShowAddAccountFormAsync() => RequestShowAddAccountForm();

        [RelayCommand]
        private Task ShowAccountOptionsAsync(BankAccountViewModel account) => RequestShowAccountOptions(account);

        [RelayCommand]
        private Task ToggleTransactionsAsync(BankAccountViewModel account) => RequestToggleTransactions(account);

        [RelayCommand]
        private Task LogoTapAsync(BankAccountViewModel account) => RequestLogoTap(account);

        // Load accounts from the database and populate the BankAccounts collection
        public async Task LoadAccountsAsync()
        {
            try
            {
                BankAccounts.Clear();

                IReadOnlyList<BankAccount> accounts =
                    await accountDataService.GetAllAccountsAsync();

                foreach (BankAccount account in accounts)
                {
                    BankAccountViewModel viewModel =
                        await bankAccountViewModelFactory.CreateAsync(account);

                    BankAccounts.Add(viewModel);
                }

                await UpdateNetWorthTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");
            }
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
        public async Task HandleToggleTransactionsAsync(BankAccountViewModel account)
        {
            try
            {
                // If there are no transactions, show an alert to the user
                if (account.Transactions.Count == 0)
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
        public async Task HandleLogoTapAsync(BankAccountViewModel account)
        {
            try
            {
                Uri? appUri = null;
                Uri? webUri = null;

                if (account is null)
                    return;

                Console.WriteLine($"Handling logo tap for account: {account.Name} (ID: {account.Model.Id})");

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

                if (appUri is not null)
                {
                    bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

                    if (canOpen)
                        await Launcher.Default.OpenAsync(appUri);
                    else
                        await dialogService.ShowAlertAsync(
                            "Error",
                            "Failed to open bank's application.");
                }

                if (webUri is not null)
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

        // Handle the addition of a new account by adding it to the BankAccounts collection and updating net worth totals
        private async Task OnAccountAddedAsync(Guid accountId)
        {
            Console.WriteLine($"Dashboard Page adding new account with ID: {accountId}.");

            // Find the account in the database and add it to the collection
            BankAccount accountModel = await accountDataService.GetAccountAsync(accountId);

            BankAccountViewModel accountViewModel =
                await bankAccountViewModelFactory.CreateAsync(accountModel);

            BankAccounts.Add(accountViewModel);

            await UpdateNetWorthTotals();

            Console.WriteLine($"Dashboard Page added new account with ID: {accountId} successfully.");
        }

        // Handle the update of an existing account by updating its properties in the BankAccounts collection and updating net worth totals
        private async Task OnAccountUpdatedAsync(Guid accountId)
        {
            Console.WriteLine($"Dashboard Page updating account with ID: {accountId}.");

            BankAccount updatedAccountModel = await accountDataService.GetAccountAsync(accountId);

            BankAccountViewModel existingAccountViewModel =
                BankAccounts.First(a => a.Model.Id == accountId);

            BankAccountViewModel updatedAccountViewModel =
                await bankAccountViewModelFactory.CreateAsync(updatedAccountModel);

            int index = BankAccounts.IndexOf(existingAccountViewModel);

            BankAccounts[index] = updatedAccountViewModel;

            await UpdateNetWorthTotals();

            Console.WriteLine($"Dashboard Page updated account with ID: {accountId} successfully.");
        }

        // Handle the deletion of an account by removing it from the BankAccounts collection and updating net worth totals
        private async Task OnAccountDeletedAsync(Guid accountId)
        {
            Console.WriteLine($"Dashboard Page deleting account with ID: {accountId}.");

            BankAccountViewModel account = BankAccounts.First(a => a.Model.Id == accountId);

            BankAccounts.Remove(account);

            await UpdateNetWorthTotals();

            Console.WriteLine($"Dashboard Page deleted account with ID: {accountId} successfully.");
        }

        // Handle the update of a transaction by updating its properties in the corresponding BankAccountViewModel's Transactions collection
        private async Task OnTransactionUpdatedAsync(
            int transactionId)
        {
            Console.WriteLine(
                $"Dashboard Page updating transaction {transactionId}.");

            Transaction? transaction =
                await accountDataService.GetTransactionAsync(transactionId);

            if (transaction is null)
                return;

            BankAccountViewModel account =
                BankAccounts.First(
                    a => a.Model.Id == transaction.BankAccountId);

            TransactionViewModel existingTransaction =
                account.Transactions.First(
                    t => t.Model.Id == transactionId);

            TransactionViewModel newTransactionViewModel =
                await transactionViewModelFactory.CreateAsync(transaction);

            int index =
                account.Transactions.IndexOf(
                    existingTransaction);

            account.Transactions[index] =
                newTransactionViewModel;

            Console.WriteLine(
                $"Dashboard Page updated transaction {transactionId} successfully.");
        }

        // Constructor for DashboardPageViewModel
        public DashboardPageViewModel(IDialogService dialogService, IAccountDataService accountDataService, IBankAccountViewModelFactory bankAccountViewModelFactory, ITransactionViewModelFactory transactionViewModelFactory, AddAccountViewModel addAccountViewModel, AccountOptionsViewModel accountOptionsViewModel)
        {
            this.dialogService = dialogService;
            this.accountDataService = accountDataService;

            this.bankAccountViewModelFactory = bankAccountViewModelFactory;
            this.transactionViewModelFactory = transactionViewModelFactory;

            AddAccountViewModel = addAccountViewModel;
            AccountOptionsViewModel = accountOptionsViewModel;

            WeakReferenceMessenger.Default.Register<
                DashboardPageViewModel,
                AccountAddedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnAccountAddedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                DashboardPageViewModel,
                AccountUpdatedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnAccountUpdatedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                DashboardPageViewModel,
                AccountDeletedMessage>(
                this,
                static (recipient, message) =>
                {
                    _ = recipient.OnAccountDeletedAsync(
                        message.Value);
                });

            WeakReferenceMessenger.Default.Register<
                DashboardPageViewModel,
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
