using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Import;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class AccountOptionsViewModel(IDialogService dialogService, IAccountDataService accountDataService, ICSVImportService csvImportService) : ObservableObject
    {
        [ObservableProperty]
        private BankAccountViewModel? selectedAccount;

        public event Func<Task>? CloseRequested;

        public event Func<BankAccountViewModel, Task>? AccountDeleted;

        public event Func<Task>? AccountBalanceChanged;

        private async Task RequestClose()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }

        // Handle the renaming of an account
        [RelayCommand]
        private async Task RenameAccount()
        {

            Console.WriteLine($"Handling rename account request for account: {SelectedAccount?.Name} (ID: {SelectedAccount?.Id})");

            BankAccountViewModel? account = SelectedAccount;

            if (account == null)
                return;

            try
            {
                await RequestClose();

                string? newName = await dialogService.ShowPromptAsync(
                    "Rename Account",
                    "Enter the new account name:",
                    account.Name);

                // If the user cancels the prompt or enters an empty name, do not proceed with renaming
                if (string.IsNullOrWhiteSpace(newName))
                    return;

                // Load existing accounts to check for duplicates
                IReadOnlyList<BankAccount> existingAccounts =
                    await accountDataService.LoadAccountsAsync();

                // Check for duplicate account based on name, bank, and type
                if (existingAccounts.Any(a =>
                a.Id != account.Id &&
                a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
                a.Institution.Equals(account.Institution) &&
                a.Type.Equals(account.Type)))
                {
                    await dialogService.ShowAlertAsync(
                    "Error",
                    "An account with this name, bank, and type already exists.");

                    return;
                }

                account.Name = newName.Trim();

                await accountDataService.SaveAccountAsync(account.Model);

                await dialogService.ShowAlertAsync(
                    "Success",
                    $"Account renamed to '{newName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming account: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "Failed to rename account. Please try again.");
            }
            finally
            {
                if (ReferenceEquals(SelectedAccount, account))
                    SelectedAccount = null;
            }
        }

        // Handle the import of transactions from a CSV file for a specific account from the AccountOptionsViewModel
        [RelayCommand]
        private async Task ImportCSV()
        {
            Console.WriteLine($"Handling CSV import for account: {SelectedAccount?.Name} (ID: {SelectedAccount?.Id})");

            BankAccountViewModel? account = SelectedAccount;

            if (account == null)
                return;

            try
            {
                await RequestClose();

                // Prompt the user to select a CSV file for import
                FileResult? file = await csvImportService.PickCSVFileAsync();

                if (file == null)
                    return;

                // Parse the bank-specific CSV into normalized transactions
                using Stream stream = await file.OpenReadAsync();

                IReadOnlyList<Transaction> importedTransactions =
                    await csvImportService.ParseTransactions(stream, account.Model);

                IReadOnlyList<Transaction> existingTransactions =
                    await accountDataService.LoadTransactionsAsync(account.Model.Id);

                // Compare the parsed rows against already known transactions, flag duplicates, and return a summary of the import results
                TransactionImportResult importResult =
                    TransactionImportProcessor.ProcessImport(
                        importedTransactions,
                        existingTransactions,
                        account.Model);

                // If there are no new transactions to add, show an alert to the user and exit the method
                if (importResult.Added.Count == 0 && importResult.PossibleDuplicates.Count == 0)
                {
                    await dialogService.ShowAlertAsync(
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
                    ImportedCount = importResult.Added.Count,
                    DuplicateCount = importResult.Duplicates.Count,
                };

                account.AddImportBatch(importBatch);

                await accountDataService.SaveImportBatchAsync(importBatch.Model);

                // Update the imported transactions with the import batch
                List<TransactionViewModel> addedTransactions = [.. importResult.Added.Select(
                    t => new TransactionViewModel(t)
                    {
                        ImportedAt = importBatch.ImportedAt
                    })];

                foreach (TransactionViewModel transaction in addedTransactions) // TODO: Add category assignment logic here
                    transaction.Model.ImportBatchId = importBatch.Id;

                await accountDataService.SaveTransactionsAsync(
                    [.. addedTransactions.Select(t => t.Model)]);

                account.AddTransactions(addedTransactions);

                await accountDataService.SaveAccountAsync(account.Model);

                // Tell DashboardViewModel that financial totals changed
                if (AccountBalanceChanged != null)
                    await AccountBalanceChanged.Invoke();

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

                await dialogService.ShowAlertAsync(
                    "Success",
                    message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing CSV: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while importing the CSV file.");
            }
            finally
            {
                if (ReferenceEquals(SelectedAccount, account))
                    SelectedAccount = null;
            }
        }

        // Handle moving the account up or down in the list of accounts
        [RelayCommand]
        private async Task MoveAccountAsync()
        {
            BankAccountViewModel? account = SelectedAccount;

            if (account == null)
                return;

            try
            {
                Console.WriteLine(
                $"Handling move account request for account: " +
                $"{account?.Name} (ID: {account?.Id})");

                // TODO:
                // Implement account ordering using a persisted
                // DisplayOrder property.

                await Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving account: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while moving the account.");
            }
            finally
            {
                if (ReferenceEquals(SelectedAccount, account))
                    SelectedAccount = null;
            }
        }

        // Handle the deletion of an account
        [RelayCommand]
        private async Task DeleteAccountAsync()
        {
            Console.WriteLine($"Handling delete account request for account: {SelectedAccount?.Name} (ID: {SelectedAccount?.Id})");

            BankAccountViewModel? account = SelectedAccount;

            if (account == null)
                return;

            try
            {
                await Close();

                // Confirm deletion with the user
                if (!await dialogService.ShowConfirmationAsync(
                    "Confirm Deletion",
                    $"Are you sure you want to delete the account '{account.Name}'?"))
                    return;

                await accountDataService.DeleteAccountAsync(account.Model.Id);

                // Tell DashboardViewModel that an account was deleted so it can update its list of accounts and recalculate totals
                if (AccountDeleted != null)
                    await AccountDeleted.Invoke(account);

                await dialogService.ShowAlertAsync(
                    "Success",
                    "Account deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while deleting the account.");
            }
            finally
            {
                if (ReferenceEquals(SelectedAccount, account))
                    SelectedAccount = null;
            }
        }

        // Handle the cancellation of the account options form
        [RelayCommand]
        private async Task Close()
        {
            Console.WriteLine("Closing Account Options form...");

            await RequestClose();

            SelectedAccount = null;
        }
    }
}