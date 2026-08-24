using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AccountDataService accountDataService;

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

        private void UpdateTotals()
        {
            NetWorth = BankAccounts.Sum(account => account.ReconciledBalance);
            Assets = BankAccounts
                .Where(account => account.ReconciledBalance > 0)
                .Sum(account => account.ReconciledBalance);
            Liabilities = BankAccounts
                .Where(account => account.ReconciledBalance < 0)
                .Sum(account => account.ReconciledBalance);

            Console.WriteLine($"Updated totals: Net Worth = {NetWorth:C}, Assets = {Assets:C}, Liabilities = {Liabilities:C}");
        }

        // Events for UI interactions
        public event Func<AddAccountViewModel, Task>? ShowAddAccountFormRequested;
        public event Func<AccountOptionsViewModel, Task>? ShowAccountOptionsFormRequested;
        public event Func<Task>? HideFormRequested;
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

        private async Task RequestShowAddAccountForm()
        {
            if (ShowAddAccountFormRequested == null)
                return;

            await ShowAddAccountFormRequested.Invoke(AddAccountViewModel);
        }

        private async Task RequestShowAccountOptionsForm(BankAccountViewModel? account)
        {
            AccountOptionsViewModel.SelectedAccount = account;

            if (ShowAccountOptionsFormRequested == null)
                return;

            await ShowAccountOptionsFormRequested.Invoke(AccountOptionsViewModel);
        }

        private async Task RequestHideForm()
        {
            if (HideFormRequested == null)
                return;

            await HideFormRequested.Invoke();
            AccountOptionsViewModel.SelectedAccount = null; // Reset the SelectedAccount after the form is closed
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
        private Task HideFormAsync() => RequestHideForm();

        [RelayCommand]
        private Task ToggleTransactionsAsync(BankAccountViewModel? account) => HandleToggleTransactions(account);

        [RelayCommand]
        private Task LogoTapAsync(BankAccountViewModel? account) => HandleLogoTap(account);

        // Constructor for DashboardViewModel
        public DashboardViewModel(AccountDataService accountDataService)
        {
            this.accountDataService = accountDataService;

            AddAccountViewModel = new AddAccountViewModel();
            AccountOptionsViewModel = new AccountOptionsViewModel();

            AddAccountViewModel.ImportCSVRequested += HandleImportCSV;
            AddAccountViewModel.AddAccountRequested += HandleAddAccount;

            AccountOptionsViewModel.RenameAccountRequested += HandleRenameAccount;
            AccountOptionsViewModel.ImportCSVRequested += HandleImportCSV;
            AccountOptionsViewModel.MoveAccountRequested += HandleMoveAccount;
            AccountOptionsViewModel.DeleteAccountRequested += HandleDeleteAccount;
        }

        // Load accounts from the database and populate the BankAccounts collection
        public async Task LoadAccountsAsync()
        {
            try
            {
                ObservableCollection<BankAccount> accounts = await accountDataService.LoadAccountsAsync();

                BankAccounts = new ObservableCollection<BankAccountViewModel>(
                    accounts.Select(a => new BankAccountViewModel(a))
                );

                UpdateTotals();
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
                await RequestHideForm();

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
                await accountDataService.SaveAccountAsync(account.Model);

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

        // Handle the import of transactions from a CSV file for a specific account from the AddAccountViewModel
        private async Task HandleImportCSV(AddAccountViewModel? viewModel)
        {
            try
            {
                if (viewModel == null)
                    return;

                // Validate that the user has selected an institution and account type before proceeding with the CSV import
                if (viewModel.SelectedInstitution == null || viewModel.SelectedType == null)
                {
                    await RequestAlert(
                        "Missing Account Information",
                        "Select an institution and account type before importing a CSV.");

                    return;
                }

                BankAccountViewModel pendingAccount = new(new BankAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = viewModel.AccountName?.Trim() ?? string.Empty,
                    Institution = (BankInstitution)viewModel.SelectedInstitution,
                    Type = (AccountType)viewModel.SelectedType,
                    ReconciledBalance = viewModel.CurrentBalance ?? 0m,
                    ReconciledThroughDate = DateTime.Now
                });

                // Prompt the user to select a CSV file for import
                FileResult? file = await CSVImportService.PickCSVFileAsync();

                // If the user cancels the file picker, exit the method without proceeding
                if (file == null)
                    return;

                // Parse the bank-specific CSV into normalized transactions
                using Stream stream = await file.OpenReadAsync();

                ObservableCollection<Transaction> importedTransactions =
                    CSVImportService.ParseTransactions(
                        stream,
                        pendingAccount.Model);

                // Compare the parsed rows against already known transactions (should be none at this point) and return a summary of the import results
                TransactionImportService.ImportResult importResult =
                    TransactionImportService.ImportTransactions(
                        importedTransactions,
                        pendingAccount.Model.Transactions,
                        pendingAccount.Model);

                // Calculate the new account balance based on the imported transactions
                pendingAccount.ReconcileBalance(importResult.Added);

                // Store the results in the AddAccountViewModel.
                viewModel.CurrentBalance = pendingAccount.ReconciledBalance;
                viewModel.PendingImport = new AddAccountViewModel.PendingCSVImport
                {
                    FileName = file.FileName,
                    Transactions = new ObservableCollection<Transaction>(importResult.Added),
                    PossibleDuplicateCount = importResult.PossibleDuplicates.Count,
                    ErrorCount = importResult.Errors.Count
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error preparing CSV import: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    "An unexpected error occurred while importing the CSV file.");
            }
        }

        // Handle the import of transactions from a CSV file for a specific account from the AccountOptionsViewModel
        private async Task HandleImportCSV(AccountOptionsViewModel? viewModel)
        {
            BankAccountViewModel? account = viewModel?.SelectedAccount;

            Console.WriteLine($"Handling CSV import for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideForm();

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

                // Compare the parsed rows against already known transactions, flag duplicates, and return a summary of the import results
                TransactionImportService.ImportResult importResult =
                    TransactionImportService.ImportTransactions(
                        importedTransactions,
                        account.Model.Transactions,
                        account.Model);

                // Create a new import batch for this CSV import
                ImportBatch importBatch = new()
                {
                    BankAccountId = account.Id,
                    FileName = file.FileName,
                    ImportedAt = DateTime.UtcNow,
                    ImportedCount = importResult.Added.Count,
                    DuplicateCount = importResult.Duplicates.Count,
                };

                await accountDataService.SaveImportBatchAsync(importBatch);

                // Update the imported transactions with the ImportBatchId and save them to the database
                foreach (Transaction transaction in importResult.Added)
                    transaction.ImportBatchId = importBatch.Id;

                if (importResult.Added.Count > 0)
                    account.UpdateTransactions(new ObservableCollection<Transaction>(importResult.Added));

                account.AddImportBatch(importBatch);

                // Calculate the new account balance based on the imported transactions
                Console.WriteLine($"Calculated balance after import: {account.ReconciledBalance:C}");

                account.ReconcileBalance(importResult.Added);
                await accountDataService.SaveAccountAsync(account.Model);

                UpdateTotals();

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

            await RequestHideForm();
        }

        // Handle the deletion of an account
        private async Task HandleDeleteAccount(AccountOptionsViewModel? viewModel)
        {
            BankAccountViewModel? account = viewModel?.SelectedAccount;

            Console.WriteLine($"Handling delete account request for account: {account?.Name} (ID: {account?.Id})");

            try
            {
                await RequestHideForm();

                if (account == null)
                    return;

                // Confirm deletion with the user
                if (await RequestAlert(
                    "Confirm Deletion",
                    $"Are you sure you want to delete the account '{account.Name}'?"))
                {
                    BankAccounts.Remove(account);
                    await accountDataService.DeleteAccountAsync(account.Model);

                    UpdateTotals();

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

        // Handle the addition of a new account
        private async Task HandleAddAccount(AddAccountViewModel? account)
        {
            Console.WriteLine("Add Account confirmation button clicked.");

            try
            {
                await RequestHideForm();

                if (account == null)
                    return;

                // Check that all required fields are filled in before proceeding
                if (string.IsNullOrWhiteSpace(account.AccountName) || account.SelectedInstitution == null || account.SelectedType == null)
                {
                    await RequestAlert(
                        "Error",
                        "All fields are required. Please fill in all details.");

                    return;
                }

                // Check for duplicate account based on name, bank, and type
                if (BankAccounts.Any(a => a.Name.Equals(account.AccountName.Trim(), StringComparison.OrdinalIgnoreCase) && a.Institution.Equals(account.SelectedInstitution) && a.Type.Equals(account.SelectedType)))
                {
                    await RequestAlert(
                        "Error",
                        "An account with this name, bank, and type already exists. Please choose a different name or modify the account details.");

                    return;
                }

                BankAccountViewModel newAccount = new(new BankAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = account.AccountName.Trim(),
                    Institution = (BankInstitution)account.SelectedInstitution,
                    Type = (AccountType)account.SelectedType,
                    ReconciledBalance = account.CurrentBalance ?? 0m, // Default to 0 if null
                    ReconciledThroughDate = DateTime.Now,
                });

                // Save the new account to the database
                await accountDataService.SaveAccountAsync(newAccount.Model);

                // Save any pending transactions that were imported from a CSV file
                if (account.PendingImport != null && account.PendingImport.Transactions.Count > 0)
                {
                    // Create and save a new import batch for the imported transactions
                    ImportBatch importBatch = new()
                    {
                        BankAccountId = newAccount.Id,
                        FileName = account.PendingImport.FileName ?? "Unknown.csv",
                        ImportedAt = DateTime.UtcNow,
                        ImportedCount = account.PendingImport.Transactions.Count,
                        DuplicateCount = 0
                    };

                    await accountDataService.SaveImportBatchAsync(importBatch);

                    // Associate transactions with this import and the new account.
                    foreach (Transaction transaction in account.PendingImport.Transactions)
                    {
                        transaction.ImportBatchId = importBatch.Id;
                        transaction.BankAccountId = newAccount.Id;
                    }

                    // Add them to the real account.
                    newAccount.UpdateTransactions(new ObservableCollection<Transaction>(account.PendingImport.Transactions));
                    newAccount.AddImportBatch(importBatch);

                    await accountDataService.SaveAccountAsync(newAccount.Model);
                }

                BankAccounts.Add(newAccount);

                UpdateTotals();

                bool importedCSV = account.PendingImport != null && account.PendingImport.Transactions.Count > 0;

                await RequestAlert(
                    "Success",
                    importedCSV
                        ? "Account and transactions added successfully."
                        : "Account added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding account: {ex.Message}\n");

                await RequestAlert(
                    "Error",
                    "An unexpected error occurred while adding the account.");
            }
        }

        // Handle toggling the visibility of transactions for a specific account
        private async Task HandleToggleTransactions(BankAccountViewModel? account)
        {
            // Console.WriteLine($"Handling toggle transactions for account: {account?.Name} (ID: {account?.Id}). Selected state: {account?.ShowTransactions}");

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
