using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Import;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class AddAccountViewModel(IDialogService dialogService, IAccountDataService accountDataService, ICSVImportService csvImportService) : ObservableObject
    {
        public class PendingCSVImport
        {
            public string FileName { get; set; } = "Unknown File";

            public required ImportBatchViewModel PendingBatch { get; set; }
        }

        private BankAccountViewModel PendingAccount = new(new BankAccount());

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPendingImport))]
        [NotifyPropertyChangedFor(nameof(ShowImportCSVButton))]
        [NotifyPropertyChangedFor(nameof(ArePickersEnabled))]
        private PendingCSVImport? pendingImport;

        [ObservableProperty]
        private string accountName = string.Empty;

        [ObservableProperty]
        private BankInstitution? selectedInstitution;

        [ObservableProperty]
        private AccountType? selectedType;

        [ObservableProperty]
        private decimal? currentBalance;

        public bool HasPendingImport => PendingImport is not null;

        public bool ShowImportCSVButton => !HasPendingImport;

        public bool ArePickersEnabled => !HasPendingImport;

        public IEnumerable<BankInstitution> BankInstitutions { get; } = Enum.GetValues<BankInstitution>().ToList();

        public IEnumerable<AccountType> AccountTypes { get; } = Enum.GetValues<AccountType>().ToList();

        public event Func<BankAccountViewModel, Task>? AccountAdded;

        public event Func<Task>? CloseRequested;

        // Handle the import of transactions from a CSV file
        [RelayCommand]
        private async Task ImportCSV()
        {
            try
            {
                // Validate that the user has selected an institution and account type before proceeding with the CSV import
                if (SelectedInstitution == null || SelectedType == null)
                {
                    await dialogService.ShowAlertAsync(
                        "Missing Account Information",
                        "Select an institution and account type before importing a CSV.");

                    return;
                }

                // Initialize the pending account with the provided details before importing transactions
                PendingAccount.Name = AccountName.Trim();
                PendingAccount.Institution = (BankInstitution)SelectedInstitution;
                PendingAccount.Type = (AccountType)SelectedType;
                PendingAccount.ReconciledBalance = CurrentBalance ?? 0m;
                PendingAccount.ReconciledThroughDate = DateTime.Now;

                // Prompt the user to select a CSV file for import
                FileResult? file = await csvImportService.PickCSVFileAsync();

                // If the user cancels the file picker, exit the method without proceeding
                if (file == null)
                    return;

                // Parse the bank-specific CSV into normalized transactions
                using Stream stream = await file.OpenReadAsync();

                IReadOnlyList<Transaction> importedTransactions =
                    await csvImportService.ParseTransactions(
                        stream,
                        PendingAccount.Model);

                // Get a summary of the import results
                TransactionImportResult importResult =
                    TransactionImportProcessor.ProcessImport(
                        importedTransactions,
                        [], // No existing transactions to compare against since this is a new account
                        PendingAccount.Model);

                PendingImport = new PendingCSVImport
                {
                    FileName = file.FileName,
                    PendingBatch = new(new ImportBatch
                    {
                        BankAccountId = PendingAccount.Id,
                        FileName = file.FileName,
                        ImportedAt = DateTime.UtcNow,
                        ImportedCount = importResult.Added.Count,
                        DuplicateCount = importResult.Duplicates.Count,
                        PossibleDuplicateCount = importResult.PossibleDuplicates.Count,
                        ErrorCount = importResult.Errors.Count
                    })
                };

                // Update the imported transactions with the import batch
                List<TransactionViewModel> addedTransactions = [.. importResult.Added.Select(
                    t => new TransactionViewModel(t)
                    {
                        ImportedAt = PendingImport.PendingBatch.ImportedAt
                    })];

                foreach (TransactionViewModel transaction in addedTransactions) // TODO: Add category assignment logic here
                    transaction.Model.ImportBatchId = PendingImport.PendingBatch.Id;

                PendingAccount.AddTransactions(addedTransactions);

                // Store the results of the import in the PendingImport property for later use when adding the account
                CurrentBalance = PendingAccount.ReconciledBalance;

                Console.WriteLine(
                    $"CSV import prepared for account '{PendingAccount.Name}' (ID: {PendingAccount.Id}). " +
                    $"Pending batch: {PendingImport.PendingBatch.ImportedCount}, " +
                    $"Possible duplicates: {PendingImport.PendingBatch.PossibleDuplicateCount}, " +
                    $"Errors: {PendingImport.PendingBatch.ErrorCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error preparing CSV import: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
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
                    await dialogService.ShowAlertAsync(
                        "Error",
                        "All fields are required. Please fill in all details.");

                    return;
                }

                // Load existing accounts to check for duplicates
                IReadOnlyList<BankAccount> existingAccounts =
                    await accountDataService.LoadAccountsAsync();

                // Check for duplicate account based on name, bank, and type
                if (existingAccounts.Any(a =>
                    a.Name.Equals(AccountName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    a.Institution.Equals(SelectedInstitution) &&
                        a.Type.Equals(SelectedType)))
                {
                    await dialogService.ShowAlertAsync(
                        "Error",
                        "An account with this name, bank, and type already exists. Please choose a different name or modify the account details.");

                    return;
                }

                // Initialize the pending account with the provided details if not already done during CSV import
                if (PendingImport == null || HasPendingImport == false)
                {
                    PendingAccount.Name = AccountName.Trim();
                    PendingAccount.Institution = (BankInstitution)SelectedInstitution;
                    PendingAccount.Type = (AccountType)SelectedType;
                    PendingAccount.ReconciledBalance = CurrentBalance ?? 0m;
                    PendingAccount.ReconciledThroughDate = DateTime.Now;
                }

                // Save the pending account to the database
                await accountDataService.SaveAccountAsync(PendingAccount.Model);

                // Save any pending transactions that were imported from a CSV file
                if (PendingImport != null && HasPendingImport == true)
                {
                    await accountDataService.SaveImportBatchAsync(PendingImport.PendingBatch.Model);

                    await accountDataService.SaveTransactionsAsync([.. PendingAccount.Transactions.Select(t => t.Model)]);
                }

                // Tell the dashboard that a new account was successfully created
                if (AccountAdded != null)
                    await AccountAdded.Invoke(PendingAccount);

                await dialogService.ShowAlertAsync(
                    "Success",
                    PendingImport != null && PendingImport.PendingBatch.ImportedCount > 0
                        ? "Account and transactions added successfully."
                        : "Account added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding account: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while adding the account.");
            }
            finally
            {
                await Close();
            }
        }

        // Handle the closing of the Add Account form, resetting the form and notifying any subscribers
        [RelayCommand]
        private async Task Close()
        {
            Reset();

            Console.WriteLine("Closing Add Account form...");

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