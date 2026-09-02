using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class BankAccountViewModel : ObservableObject
    {
        public BankAccount Model { get; }

        public string Name
        {
            get => Model.Name;
            set => SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
        }

        public BankInstitution Institution
        {
            get => Model.Institution;
            set => SetProperty(Model.Institution, value, Model, (m, v) => m.Institution = v);
        }

        public AccountType Type
        {
            get => Model.Type;
            set => SetProperty(Model.Type, value, Model, (m, v) => m.Type = v);
        }

        public decimal ReconciledBalance
        {
            get => Model.ReconciledBalance;
            set => SetProperty(Model.ReconciledBalance, value, Model, (m, v) => m.ReconciledBalance = v);
        }

        public DateTime ReconciledThroughDate
        {
            get => Model.ReconciledThroughDate;
            set => SetProperty(Model.ReconciledThroughDate, value, Model, (m, v) => m.ReconciledThroughDate = v);
        }

        public ObservableCollection<TransactionViewModel> Transactions { get; } = [];

        [ObservableProperty]
        private IReadOnlyList<TransactionGroupViewModel> transactionGroups = [];

        private List<ImportBatchViewModel> ImportBatches { get; } = [];

        [ObservableProperty]
        private ImportBatchViewModel? lastImport;

        [ObservableProperty]
        private bool showTransactions;

        [ObservableProperty]
        private int displayOrder;

        private bool suppressTransactionGroupRefresh;

        // Calculates the account balance from the last reconciliation snapshot and updates the account's reconciled balance and date
        private void ReconcileBalance(IEnumerable<TransactionViewModel> addedTransactions)
        {
            decimal changeSinceReconciliation = addedTransactions
                .Where(t =>
                    t.Model.BankAccountId == Model.Id &&
                    t.Date.Date > Model.ReconciledThroughDate.Date)
                .Sum(t => t.Amount); // only sum transactions that occurred after the last reconciliation date

            decimal calculatedBalance = Model.ReconciledBalance + changeSinceReconciliation;

            ReconciledBalance = calculatedBalance;
            ReconciledThroughDate = addedTransactions.Any() ? addedTransactions.Max(t => t.Date) : ReconciledThroughDate; // update the reconciled date to the latest transaction date if there are any added transactions
        }

        // Add a collection of import batches to the account and refresh the last import batch
        public void AddImportBatch(ImportBatchViewModel importBatch)
        {
            ImportBatches.Add(importBatch);
            LastImport = importBatch;
        }

        // Add a collection of transactions to the account and refresh the transaction groups
        public void AddTransactions(IEnumerable<TransactionViewModel> transactions)
        {
            suppressTransactionGroupRefresh = true;

            foreach (TransactionViewModel transaction in transactions)
                Transactions.Add(transaction);

            suppressTransactionGroupRefresh = false;

            RefreshTransactionGroups();

            ReconcileBalance(transactions);

            ShowTransactions = true;
        }

        // Refresh the transaction groups based on the current transactions, grouping them by date and ordering them in descending order
        private void RefreshTransactionGroups()
        {
            TransactionGroups =
            [
                .. Transactions
                    .GroupBy(t => t.Date.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g =>
                        new TransactionGroupViewModel(
                            g.Key,
                            g.OrderByDescending(t => t.Date)))
            ];
        }

        private void OnTransactionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!suppressTransactionGroupRefresh)
                RefreshTransactionGroups();
        }

        // Constructor for BankAccountViewModel
        public BankAccountViewModel(BankAccount model)
        {
            Model = model;

            Transactions.CollectionChanged +=
                OnTransactionsCollectionChanged;
        }
    }
}