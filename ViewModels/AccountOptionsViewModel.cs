using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class AccountOptionsViewModel : ObservableObject
    {
        [ObservableProperty]
        private BankAccountViewModel? selectedAccount;

        public event Func<string, string, Task>? ShowAlertRequested;

        public event Func<string, string, string?, Task<string?>>? ShowPromptRequested;

        public event Func<string, string, Task<bool>>? ShowConfirmationRequested;

        public event Func<Task>? CloseRequested;

        public event Func<BankAccountViewModel, Task>? AccountDeleted;

        public event Func<Task>? AccountBalanceChanged;

        private async Task RequestAlert(string title, string message)
        {
            if (ShowAlertRequested != null)
                await ShowAlertRequested.Invoke(title, message);
        }

        private async Task<string?> RequestPrompt(string title, string message, string? initialValue = null)
        {
            if (ShowPromptRequested == null)
                return null;

            return await ShowPromptRequested.Invoke(
                title,
                message,
                initialValue);
        }

        private async Task<bool> RequestConfirmation(string title, string message)
        {
            if (ShowConfirmationRequested == null)
                return false;

            return await ShowConfirmationRequested.Invoke(
                title,
                message);
        }

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

                string? newName = await RequestPrompt(
                    "Rename Account",
                    "Enter the new account name:",
                    account.Name);

                // If the user cancels the prompt or enters an empty name, do not proceed with renaming
                if (string.IsNullOrWhiteSpace(newName))
                    return;

                // Load existing accounts to check for duplicates
                ObservableCollection<BankAccount> existingAccounts =
                    await AccountDataService.LoadAccountsAsync();

                // Check for duplicate account based on name, bank, and type
                if (existingAccounts.Any(a =>
                a.Id != account.Id &&
                a.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
                a.Institution.Equals(account.Institution) &&
                a.Type.Equals(account.Type)))
                {
                    await RequestAlert(
                    "Error",
                    "An account with this name, bank, and type already exists.");

                    return;
                }

                account.Name = newName.Trim();

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
                    importResult.Added.Select(
                        t => new TransactionViewModel(t)));

                foreach (TransactionViewModel transaction in transactionsToSave) // TODO: Add category assignment logic here if needed
                {
                    transaction.Model.ImportBatchId = importBatch.Id;
                    transaction.ImportedAt = importBatch.ImportedAt;
                    transaction.BankAccountId = account.Id;
                }

                await AccountDataService.SaveTransactionsAsync(
                    new ObservableCollection<Transaction>(
                        transactionsToSave.Select(t => t.Model)));

                account.AddTransactions(transactionsToSave);

                // Reconcile the account's balance based on the newly imported transactions
                account.ReconcileBalance(transactionsToSave);

                await AccountDataService.SaveAccountAsync(account.Model);

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

                await RequestAlert(
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
                if (!await RequestConfirmation(
                    "Confirm Deletion",
                    $"Are you sure you want to delete the account '{account.Name}'?"))
                    return;

                await AccountDataService.DeleteAccountAsync(account.Model);

                // Tell DashboardViewModel that an account was deleted so it can update its list of accounts and recalculate totals
                if (AccountDeleted != null)
                    await AccountDeleted.Invoke(account);

                await RequestAlert(
                    "Success",
                    "Account deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}\n");

                await RequestAlert(
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