using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using trackr.Factories;
using trackr.Import;
using trackr.Messages;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class AccountOptionsViewModel(IDialogService dialogService, IAccountDataService accountDataService, ICSVImportService csvImportService,
    ITransactionViewModelFactory transactionViewModelFactory) : ObservableObject
    {
        [ObservableProperty]
        private BankAccountViewModel? selectedAccount;

        public event Func<Task>? CloseRequested;

        private async Task RequestClose()
        {
            if (CloseRequested is not null)
                await CloseRequested.Invoke();
        }

        // Handle the renaming of an account
        [RelayCommand]
        private async Task RenameAccount()
        {
            Console.WriteLine($"Handling rename account request for account: {SelectedAccount?.Name} (ID: {SelectedAccount?.Model.Id})");

            if (SelectedAccount is not BankAccountViewModel account)
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

                // Check for duplicate account based on name, bank, and type
                if (await accountDataService.AccountExistsAsync(account.Model))
                {
                    await dialogService.ShowAlertAsync(
                        "Duplicate Account",
                        "An account with this name, bank, and type already exists. Please choose a different name.");

                    await RenameAccount(); // Prompt the user again for a new name
                    return;
                }

                account.Name = newName.Trim();

                await accountDataService.UpdateAccountAsync(account.Model);

                // Tell the rest of the application that an account was updated.
                WeakReferenceMessenger.Default.Send(
                    new AccountUpdatedMessage(
                        account.Model.Id));

                await dialogService.ShowAlertAsync(
                    "Success",
                    $"Account renamed to '{newName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming account: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while renaming the account.");
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
            Console.WriteLine($"Handling CSV import for account: {SelectedAccount?.Name} (ID: {SelectedAccount?.Model.Id})");

            if (SelectedAccount is not BankAccountViewModel account)
                return;

            try
            {
                await RequestClose();

                // Prompt the user to select a CSV file for import
                FileResult? file = await csvImportService.PickCSVFileAsync();

                if (file is null)
                    return;

                // Parse the bank-specific CSV into normalized transactions
                using Stream stream = await file.OpenReadAsync();

                IReadOnlyList<Transaction> importedTransactions =
                    await csvImportService.ParseTransactions(stream, account.Model);

                IReadOnlyList<Transaction> existingTransactions =
                    await accountDataService.GetTransactionsForAccountAsync(account.Model.Id);

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
                    BankAccountId = account.Model.Id,
                    FileName = file.FileName,
                    ImportedAt = DateTime.UtcNow,
                    ImportedCount = importResult.Added.Count,
                    DuplicateCount = importResult.Duplicates.Count,
                    PossibleDuplicateCount = importResult.PossibleDuplicates.Count,
                    ErrorCount = importResult.Errors.Count
                });

                await accountDataService.InsertImportBatchAsync(importBatch.Model);

                account.AddImportBatch(importBatch);

                List<TransactionViewModel> addedTransactions = [];

                // Create TransactionViewModel instances for each added transaction and associate them with the import batch
                foreach (Transaction model in importResult.Added)
                {
                    model.ImportBatchId = importBatch.Model.Id;

                    TransactionViewModel transaction =
                        await transactionViewModelFactory.CreateAsync(model);

                    addedTransactions.Add(transaction);

                    await accountDataService.InsertTransactionAsync(transaction.Model);
                }

                await accountDataService.UpdateAccountAsync(account.Model);

                // Tell the rest of the application that the account was updated with the new transactions.
                WeakReferenceMessenger.Default.Send(
                    new AccountUpdatedMessage(
                        account.Model.Id));

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
            if (SelectedAccount is not BankAccountViewModel account)
                return;

            try
            {
                Console.WriteLine(
                $"Handling move account request for account: " +
                $"{account?.Name} (ID: {account?.Model.Id})");

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
            Console.WriteLine($"Handling delete account request for account: {SelectedAccount?.Name} (ID: {SelectedAccount?.Model.Id})");

            if (SelectedAccount is not BankAccountViewModel account)
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

                // Tell the rest of the application that an account was deleted
                WeakReferenceMessenger.Default.Send(
                    new AccountDeletedMessage(
                        account.Model.Id));

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