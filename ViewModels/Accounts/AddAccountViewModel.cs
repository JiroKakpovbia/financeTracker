using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using trackr.Import;
using trackr.Models;
using trackr.Services;

namespace trackr.ViewModels
{
    public partial class AddAccountViewModel(IDialogService dialogService, IAccountDataService accountDataService, ICSVImportService csvImportService,
    ITransactionViewModelFactory transactionViewModelFactory) : ObservableObject
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
                if (SelectedInstitution is null ||
                    SelectedType is null)
                {
                    string message = "Select the following fields:";

                    if (SelectedInstitution is null)
                        message += "\n- Bank Institution";

                    if (SelectedType is null)
                        message += "\n- Account Type";

                    await dialogService.ShowAlertAsync(
                        "Missing Account Information",
                        message);

                    return;
                }

                // Initialize the pending account with the provided details before importing transactions
                PendingAccount.Name = AccountName.Trim();
                PendingAccount.Institution = SelectedInstitution.Value;
                PendingAccount.Type = SelectedType.Value;
                PendingAccount.ReconciledBalance = CurrentBalance ?? 0m;
                PendingAccount.ReconciledThroughDate = DateTime.Now;

                // Prompt the user to select a CSV file for import
                FileResult? file = await csvImportService.PickCSVFileAsync();

                // If the user cancels the file picker, exit the method without proceeding
                if (file is null)
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

                // Create TransactionViewModel instances for each added transaction and associate them with the import batch
                List<TransactionViewModel> addedTransactions = [];

                foreach (Transaction model in importResult.Added)
                {
                    TransactionViewModel transaction =
                        await transactionViewModelFactory.CreateAsync(model);

                    transaction.AccountName = PendingAccount.Name;
                    transaction.AccountInstitution = PendingAccount.Institution;
                    transaction.ImportedAt = PendingImport.PendingBatch.ImportedAt;

                    transaction.Model.ImportBatchId =
                        PendingImport.PendingBatch.Id;

                    addedTransactions.Add(transaction);
                }

                PendingAccount.AddImportBatch(PendingImport.PendingBatch);
                PendingAccount.AddTransactions(addedTransactions);

                // Update the current balance to reflect the reconciled balance of the pending account
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
                    SelectedInstitution is null ||
                    SelectedType is null)
                {
                    string message = "All fields are required. Please fill in all details.";

                    if (string.IsNullOrWhiteSpace(AccountName))
                        message += "\n- Account Name is missing.";

                    if (SelectedInstitution is null)
                        message += "\n- Bank Institution is not selected.";

                    if (SelectedType is null)
                        message += "\n- Account Type is not selected.";

                    await dialogService.ShowAlertAsync(
                        "Error",
                        message);

                    return;
                }

                PendingAccount.Name = AccountName.Trim();
                PendingAccount.Institution = SelectedInstitution.Value;
                PendingAccount.Type = SelectedType.Value;
                PendingAccount.ReconciledBalance = CurrentBalance ?? 0m;
                PendingAccount.ReconciledThroughDate = DateTime.Now;

                // Check for duplicate account based on name, bank, and type
                if (await accountDataService.AccountExistsAsync(PendingAccount.Model))
                {
                    await dialogService.ShowAlertAsync(
                        "Duplicate Account",
                        "An account with this name, bank, and type already exists. Please choose a different name or modify the account details.");

                    return;
                }

                // Save the pending account to the database
                await accountDataService.SaveAccountAsync(PendingAccount.Model);

                // Save any pending transactions that were imported from a CSV file
                if (PendingImport is not null)
                {
                    await accountDataService.SaveImportBatchAsync(PendingImport.PendingBatch.Model);

                    // Update the ImportBatchId for each transaction to link them to the saved import batch
                    foreach (TransactionViewModel transaction in PendingAccount.Transactions)
                        transaction.Model.ImportBatchId = PendingImport.PendingBatch.Id;

                    await accountDataService.SaveTransactionsAsync([.. PendingAccount.Transactions.Select(t => t.Model)]);
                }

                // Tell the dashboard that a new account was successfully created
                if (AccountAdded is not null)
                    await AccountAdded.Invoke(PendingAccount);

                await dialogService.ShowAlertAsync(
                    "Success",
                    PendingImport is not null
                        ? "Account and transactions added successfully."
                        : "Account added successfully.");

                await Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding account: {ex.Message}\n");

                await dialogService.ShowAlertAsync(
                    "Error",
                    "An unexpected error occurred while adding the account.");
            }
        }

        // Handle the closing of the Add Account form, resetting the form and notifying any subscribers
        [RelayCommand]
        private async Task Close()
        {
            Reset();

            Console.WriteLine("Closing Add Account form...");

            if (CloseRequested is not null)
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