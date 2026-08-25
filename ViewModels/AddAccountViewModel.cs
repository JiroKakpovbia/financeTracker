using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class AddAccountViewModel : ObservableObject
    {
        public class PendingCSVImport
        {
            public Guid AccountId { get; set; }

            public string FileName { get; set; } = "Unknown File";

            public ObservableCollection<Transaction> Transactions { get; set; } = [];

            public int PossibleDuplicateCount { get; set; }

            public int ErrorCount { get; set; }
        }

        [ObservableProperty]
        private string accountName = string.Empty;

        [ObservableProperty]
        private BankInstitution? selectedInstitution;

        [ObservableProperty]
        private AccountType? selectedType;

        [ObservableProperty]
        private decimal? currentBalance;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPendingImport))]
        [NotifyPropertyChangedFor(nameof(ShowImportCSVButton))]
        [NotifyPropertyChangedFor(nameof(ArePickersEnabled))]
        private PendingCSVImport? pendingImport;

        public bool HasPendingImport => PendingImport is not null;

        public bool ShowImportCSVButton => !HasPendingImport;

        public bool ArePickersEnabled => !HasPendingImport;

        public IEnumerable<BankInstitution> BankInstitutions { get; } = Enum.GetValues<BankInstitution>().ToList();

        public IEnumerable<AccountType> AccountTypes { get; } = Enum.GetValues<AccountType>().ToList();

        public event Func<BankAccountViewModel, Task>? AccountAdded;

        public event Func<string, string, Task>? ShowAlertRequested;

        public event Func<Task>? CloseRequested;

        private async Task RequestAlert(string title, string message)
        {
            if (ShowAlertRequested != null)
                await ShowAlertRequested.Invoke(title, message);
        }

        // Handle the import of transactions from a CSV file
        [RelayCommand]
        private async Task ImportCSV()
        {
            try
            {
                // Validate that the user has selected an institution and account type before proceeding with the CSV import
                if (SelectedInstitution == null || SelectedType == null)
                {
                    await RequestAlert(
                        "Missing Account Information",
                        "Select an institution and account type before importing a CSV.");

                    return;
                }

                BankAccountViewModel pendingAccount = new(new BankAccount
                {
                    Name = AccountName.Trim(),
                    Institution = (BankInstitution)SelectedInstitution,
                    Type = (AccountType)SelectedType,
                    ReconciledBalance = CurrentBalance ?? 0m,
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

                // Get a summary of the import results
                TransactionImportService.ImportResult importResult =
                    TransactionImportService.ImportTransactions(
                        importedTransactions,
                        [], // No existing transactions to compare against since this is a new account
                        pendingAccount.Model);

                // Calculate the new account balance based on the imported transactions
                pendingAccount.ReconcileBalance(
                    importResult.Added.Select(t => new TransactionViewModel(t)));

                // Store the imported transactions in a new ObservableCollection and associate them with the pending account
                ObservableCollection<Transaction> transactionsToSave =
                    new(importResult.Added);

                foreach (Transaction transaction in transactionsToSave)
                    transaction.BankAccountId = pendingAccount.Id;

                // Store the results of the CSV import in the PendingImport property for later use when the account is added
                CurrentBalance = pendingAccount.ReconciledBalance;

                PendingImport = new PendingCSVImport
                {
                    AccountId = pendingAccount.Id,
                    FileName = file.FileName,
                    Transactions = transactionsToSave,
                    PossibleDuplicateCount = importResult.PossibleDuplicates.Count,
                    ErrorCount = importResult.Errors.Count
                };

                Console.WriteLine(
                    $"CSV import prepared for account '{pendingAccount.Name}' (ID: {pendingAccount.Id}). " +
                    $"Pending transactions: {PendingImport.Transactions.Count}, " +
                    $"Possible duplicates: {PendingImport.PossibleDuplicateCount}, " +
                    $"Errors: {PendingImport.ErrorCount}");
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

        // Handle the addition of a new account
        [RelayCommand]
        private async Task AddAccount()
        {
            try
            {
                // Check that all required fields are filled in before proceeding
                if (string.IsNullOrWhiteSpace(AccountName) ||
                    SelectedInstitution == null ||
                    SelectedType == null)
                {
                    await RequestAlert(
                        "Error",
                        "All fields are required. Please fill in all details.");

                    return;
                }

                // Load existing accounts to check for duplicates
                ObservableCollection<BankAccount> existingAccounts =
                    await AccountDataService.LoadAccountsAsync();

                // Check for duplicate account based on name, bank, and type
                if (existingAccounts.Any(a => a.Name.Equals(AccountName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    a.Institution.Equals(SelectedInstitution) &&
                        a.Type.Equals(SelectedType)))
                {
                    await RequestAlert(
                        "Error",
                        "An account with this name, bank, and type already exists. Please choose a different name or modify the account details.");

                    return;
                }

                BankAccountViewModel newAccount = new(new BankAccount
                {
                    Id = PendingImport?.AccountId ?? Guid.NewGuid(), // Use the AccountId from PendingImport if available
                    Name = AccountName.Trim(),
                    Institution = (BankInstitution)SelectedInstitution,
                    Type = (AccountType)SelectedType,
                    ReconciledBalance = CurrentBalance ?? 0m, // Default to 0 if null
                    ReconciledThroughDate = DateTime.Now,
                });

                // Save the new account to the database
                await AccountDataService.SaveAccountAsync(newAccount.Model);

                // Save any pending transactions that were imported from a CSV file
                if (PendingImport != null && PendingImport.Transactions.Count > 0)
                {
                    // Create and save a new import batch for the imported transactions
                    ImportBatchViewModel importBatch = new(new ImportBatch
                    {
                        BankAccountId = newAccount.Id,
                        FileName = PendingImport.FileName,
                        ImportedAt = DateTime.UtcNow,
                    })
                    {
                        ImportedCount = PendingImport.Transactions.Count,
                        DuplicateCount = 0
                    };

                    await AccountDataService.SaveImportBatchAsync(importBatch.Model);

                    // Update the account's import batches and last import reference
                    newAccount.ImportBatches.Add(importBatch);
                    newAccount.LastImport = importBatch;

                    // Associate transactions with this import and the new account
                    ObservableCollection<TransactionViewModel> transactionsToSave = new(
                    PendingImport.Transactions.Select(t => new TransactionViewModel(t)));

                    foreach (TransactionViewModel transaction in transactionsToSave)
                    {
                        transaction.Model.ImportBatchId = importBatch.Id;
                        transaction.ImportedAt = importBatch.ImportedAt;
                        transaction.BankAccountId = newAccount.Id;
                    }

                    await AccountDataService.SaveTransactionsAsync(new ObservableCollection<Transaction>(transactionsToSave.Select(t => t.Model)));

                    newAccount.AddTransactions(transactionsToSave);
                }

                // Tell the dashboard that a new account was successfully created
                if (AccountAdded != null)
                    await AccountAdded.Invoke(newAccount);

                await RequestAlert(
                    "Success",
                    PendingImport != null && PendingImport.Transactions.Count > 0
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
            finally
            {
                Reset(); // Ensure the form is reset after attempting to add an account, regardless of success or failure

                // Close the Add Account page
                if (CloseRequested != null)
                    await CloseRequested.Invoke();
            }
        }

        // Handle the cancellation of adding a new account
        [RelayCommand]
        private async Task Cancel()
        {
            Reset();

            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }

        [RelayCommand]
        private void ClearPendingImport()
        {
            PendingImport = null;
            CurrentBalance = null;
        }

        public void Reset()
        {
            AccountName = string.Empty;
            SelectedInstitution = null;
            SelectedType = null;
            CurrentBalance = null;

            ClearPendingImport();
        }
    }
}