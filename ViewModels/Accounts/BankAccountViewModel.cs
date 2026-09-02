using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class BankAccountViewModel(BankAccount model) : ObservableObject
    {
        public BankAccount Model { get; } = model;

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

        public ObservableCollection<TransactionGroupViewModel> TransactionGroups { get; } = [];

        private ObservableCollection<ImportBatchViewModel> ImportBatches { get; } = [];


        [ObservableProperty]
        private ImportBatchViewModel? lastImport;

        [ObservableProperty]
        private bool showTransactions;

        [ObservableProperty]
        private int displayOrder;

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
            foreach (TransactionViewModel transaction in transactions)
                Transactions.Add(transaction);

            RefreshTransactionGroups();

            ReconcileBalance(transactions);

            ShowTransactions = true;
        }

        public void UpdateTransaction(TransactionViewModel updatedTransaction)
        {
            int index = Transactions.IndexOf(
                Transactions.First(t => t.Model.Id == updatedTransaction.Model.Id));

            if (index >= 0)
            {
                Transactions[index] = updatedTransaction;
                
                RefreshTransactionGroups();
            }
        }

        // Refresh the transaction groups based on the current transactions, grouping them by date and ordering them in descending order
        private void RefreshTransactionGroups()
        {
            TransactionGroups.Clear();

            List<TransactionGroupViewModel> newGroups = [.. Transactions
                .GroupBy(t => t.Date.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroupViewModel(
                    g.Key,
                    g.OrderByDescending(t => t.Date)
                ))];

            foreach (TransactionGroupViewModel group in newGroups)
                TransactionGroups.Add(group);
        }
    }
}