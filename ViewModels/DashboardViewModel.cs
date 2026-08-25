using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
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
        public event Func<AddAccountViewModel, Task>? HideAddAccountFormRequested;
        public event Func<AccountOptionsViewModel, Task>? ShowAccountOptionsFormRequested;
        public event Func<AccountOptionsViewModel, Task>? HideAccountOptionsFormRequested;
        public event Func<object?, AlertEventArgs, Task<bool>>? ShowAlertRequested;
        public event Func<object?, PromptEventArgs, Task<string?>>? ShowPromptRequested;

        // Request methods for UI interactions
        private async Task<bool> RequestAlert(string title, string message)
        {
            if (ShowAlertRequested == null)
                return false;

            return await ShowAlertRequested.Invoke(this, new AlertEventArgs(title, message));
        }

        private async Task<string?> RequestPrompt(string title, string message, string? initialValue)
        {
            if (ShowPromptRequested == null)
                return null;

            return await ShowPromptRequested.Invoke(this, new PromptEventArgs(title, message, initialValue));
        }

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

        // Request to hide the Account Options form
        private async Task RequestHideAccountOptionsForm()
        {
            if (HideAccountOptionsFormRequested != null)
                await HideAccountOptionsFormRequested.Invoke(AccountOptionsViewModel);

            AccountOptionsViewModel.SelectedAccount = null; // Clear the selected account after hiding the form
        }

        // Event argument classes for alerts and prompts
        public class AlertEventArgs(string title, string message) : EventArgs
        {
            public string Title { get; } = title;
            public string Message { get; } = message;
        }

        public class PromptEventArgs(string title, string message, string? initialValue) : EventArgs
        {
            public string Title { get; } = title;
            public string Message { get; } = message;
            public string? InitialValue { get; } = initialValue;
        }

        [RelayCommand]
        private Task ShowAddAccountFormAsync() => RequestShowAddAccountForm();

        [RelayCommand]
        private Task ShowAccountOptionsAsync(BankAccountViewModel? account) => RequestShowAccountOptionsForm(account);

        [RelayCommand]
        private Task HideAccountOptionsFormAsync() => RequestHideAccountOptionsForm();

        [RelayCommand]
        private Task ToggleTransactionsAsync(BankAccountViewModel? account) => HandleToggleTransactions(account);

        [RelayCommand]
        private Task LogoTapAsync(BankAccountViewModel? account) => HandleLogoTap(account);

        // Constructor for DashboardViewModel
        public DashboardViewModel()
        {
            AddAccountViewModel = new AddAccountViewModel();
            AccountOptionsViewModel = new AccountOptionsViewModel();

            AddAccountViewModel.AccountAdded += OnAccountAdded;

            AccountOptionsViewModel.RenameAccountRequested += HandleRenameAccount;
            AccountOptionsViewModel.ImportCSVRequested += HandleImportCSV;
            AccountOptionsViewModel.MoveAccountRequested += HandleMoveAccount;
            AccountOptionsViewModel.DeleteAccountRequested += HandleDeleteAccount;
        }

        private void UpdateNetWorthTotals()
        {
            NetWorth = BankAccounts.Sum(account => account.ReconciledBalance);
            Assets = BankAccounts
                .Where(account => account.ReconciledBalance > 0)
                .Sum(account => account.ReconciledBalance);
            Liabilities = BankAccounts
                .Where(account => account.ReconciledBalance < 0)
                .Sum(account => account.ReconciledBalance);
        }

        // Load accounts from the database and populate the BankAccounts collection
        public async Task LoadAccountsAsync()
        {
            try
            {
                ObservableCollection<BankAccount> accounts = await AccountDataService.LoadAccountsAsync();

                BankAccounts = new ObservableCollection<BankAccountViewModel>(
                    accounts.Select(a => new BankAccountViewModel(a))
                );

                foreach (BankAccountViewModel account in BankAccounts)
                {
                    ObservableCollection<Transaction> transactions = await AccountDataService.LoadTransactionsAsync(account.Model);
                    ObservableCollection<ImportBatch> importBatches = await AccountDataService.LoadImportBatchesAsync(account.Model);

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
                }

                UpdateNetWorthTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading accounts: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    "Failed to load accounts. Please try again.");
            }
        }

        // Handle the renaming of an account
        private async Task HandleRenameAccount(AccountOptionsViewModel? viewModel)
        {
            BankAccountViewModel? account = viewModel?.SelectedAccount;

            Console.WriteLine($"Handling rename account request for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideAccountOptionsForm();

                if (account == null)
                    return;

                string? newName = await RequestPrompt("Rename Account", "Enter the new account name:", account.Name);

                // If the user cancels the prompt or enters an empty name, do not proceed with renaming
                if (string.IsNullOrWhiteSpace(newName))
                    return;

                // Check for duplicate account based on name, bank, and type
                if (BankAccounts.Any(a => a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && a.Institution.Equals(account.Institution) && a.Type.Equals(account.Type)))
                {
                    await RequestAlert(
                    "Error",
                    "An account with this name, bank, and type already exists.");
                    return;
                }

                account.Name = newName;
                await AccountDataService.SaveAccountAsync(account.Model);

                await RequestAlert(
                    "Success",
                    $"Account renamed to '{newName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming account: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    "Failed to rename account. Please try again.");
            }
        }

        // Handle the import of transactions from a CSV file for a specific account from the AccountOptionsViewModel
        private async Task HandleImportCSV(AccountOptionsViewModel? viewModel)
        {
            BankAccountViewModel? account = viewModel?.SelectedAccount;

            Console.WriteLine($"Handling CSV import for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideAccountOptionsForm();

                if (account == null)
                    return;

                // Prompt the user to select a CSV file for import
                FileResult? file = await CSVImportService.PickCSVFileAsync();

                if (file == null)
                    return;

                // Parse the bank-specific CSV into normalized transactions
                using Stream stream = await file.OpenReadAsync();

                ObservableCollection<Transaction> importedTransactions =
                    CSVImportService.ParseTransactions(stream, account.Model);

                ObservableCollection<Transaction> existingTransactions =
                    await AccountDataService.LoadTransactionsAsync(account.Model);

                // Compare the parsed rows against already known transactions, flag duplicates, and return a summary of the import results
                TransactionImportService.ImportResult importResult =
                    TransactionImportService.ImportTransactions(
                        importedTransactions,
                        existingTransactions,
                        account.Model);

                // If there are no new transactions to add, show an alert to the user and exit the method
                if (importResult.Added.Count == 0 && importResult.PossibleDuplicates.Count == 0)
                {
                    await RequestAlert(
                        "No New Transactions",
                        "The CSV import did not contain any new transactions to add.");
                    return;
                }

                // Create a new import batch for this CSV import
                ImportBatchViewModel importBatch = new(new ImportBatch
                {
                    BankAccountId = account.Id,
                    FileName = file.FileName,
                    ImportedAt = DateTime.UtcNow,
                })
                {
                    ImportedCount = importResult.Added.Count,
                    DuplicateCount = importResult.Duplicates.Count
                };

                // Update the account's import batches and last import reference
                account.ImportBatches.Add(importBatch);
                account.LastImport = importBatch;

                await AccountDataService.SaveImportBatchAsync(importBatch.Model);

                // Update the imported transactions with the import batch ID and the account ID, then save them to the database
                ObservableCollection<TransactionViewModel> transactionsToSave = new(
                    importResult.Added.Select(t => new TransactionViewModel(t)));

                foreach (TransactionViewModel transaction in transactionsToSave) // TODO: Add category assignment logic here if needed
                {
                    transaction.Model.ImportBatchId = importBatch.Id;
                    transaction.ImportedAt = importBatch.ImportedAt;
                    transaction.BankAccountId = account.Id;
                }

                await AccountDataService.SaveTransactionsAsync(new ObservableCollection<Transaction>(transactionsToSave.Select(t => t.Model)));

                account.AddTransactions(transactionsToSave);

                // Reconcile the account's balance based on the newly imported transactions
                account.ReconcileBalance(transactionsToSave);

                await AccountDataService.SaveAccountAsync(account.Model);

                UpdateNetWorthTotals();

                // Display a summary of the import results to the user
                string message =
                    $"CSV import complete.\n\n" +
                    $"{importedTransactions.Count} transactions found\n" +
                    $"{importResult.Added.Count} new transactions added\n" +
                    $"{importResult.Duplicates.Count} duplicates skipped";

                if (importResult.PossibleDuplicates.Count > 0)
                    message += $"\n{importResult.PossibleDuplicates.Count} possible duplicates imported for review";

                if (importResult.Errors.Count > 0)
                    message += $"\n{importResult.Errors.Count} rows could not be imported";

                await RequestAlert(
                    "Success",
                    message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing CSV: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    "An unexpected error occurred while importing the CSV file.");
            }
        }

        // Handle moving an account up or down in the list
        private async Task HandleMoveAccount(AccountOptionsViewModel? viewModel)
        {
            BankAccountViewModel? account = viewModel?.SelectedAccount;

            Console.WriteLine($"Handling move account request for account: {account?.Name} (ID: {account?.Id})");

            // TODO: Implement move account functionality using DisplayOrder property for sorting instead of relying on the ObservableCollection order
            // try
            // {
            //     int currentIndex = BankAccounts.IndexOf(account);
            //     int newIndex = currentIndex + direction;

            //     if (newIndex >= 0 && newIndex < BankAccounts.Count)
            //     {
            //         BankAccounts.Move(currentIndex, newIndex);
            //         await accountDataService.SaveAccountAsync(account);
            //     }
            //     else await RequestAlert("Error", "Cannot move account further than the top or bottom.");
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"Error moving account: {ex.Message}\n");
            //     await RequestAlert("Error", "An unexpected error occurred while processing your request.");
            // }

            await RequestHideAccountOptionsForm();
        }

        // Handle the deletion of an account
        private async Task HandleDeleteAccount(AccountOptionsViewModel? viewModel)
        {
            BankAccountViewModel? account = viewModel?.SelectedAccount;

            Console.WriteLine($"Handling delete account request for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideAccountOptionsForm();

                if (account == null)
                    return;

                // Confirm deletion with the user
                if (await RequestAlert(
                    "Confirm Deletion",
                    $"Are you sure you want to delete the account '{account.Name}'?"))
                {
                    BankAccounts.Remove(account);
                    await AccountDataService.DeleteAccountAsync(account.Model);

                    UpdateNetWorthTotals();

                    await RequestAlert(
                        "Success",
                        "Account deleted successfully.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    "An unexpected error occurred while deleting the account.");
            }
        }

        private Task OnAccountAdded(BankAccountViewModel account)
        {
            BankAccounts.Add(account);

            UpdateNetWorthTotals();

            return Task.CompletedTask;
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
                    await RequestAlert(
                        "No Transactions",
                        "This account has no transactions to show. Import a CSV to populate this account.");

                    return;
                }

                account.ShowTransactions = !account.ShowTransactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling transactions: {ex.Message}\n");

                await RequestAlert(
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
                    await RequestAlert(
                    "Error",
                    "Failed to open bank's website or application.");
                }

                if (appUri != null)
                {
                    bool canOpen = await Launcher.Default.CanOpenAsync(appUri);

                    if (canOpen)
                        await Launcher.Default.OpenAsync(appUri);
                    else
                        await RequestAlert(
                            "Error",
                            "Failed to open bank's website or application.");
                }
                else if (webUri != null)
                {
                    bool canOpen = await Launcher.Default.CanOpenAsync(webUri);

                    if (canOpen)
                        await Launcher.Default.OpenAsync(webUri);
                    else
                        await RequestAlert(
                            "Error",
                            "Failed to open bank's website or application.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening bank's website or application: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
