using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using trackr.Models;

namespace trackr.ViewModels
{
    public partial class BankAccountViewModel : ObservableObject
    {
        public BankAccount Model { get; }

        public string Id => Model.Id;

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

        public decimal Balance
        {
            get => Model.Balance;
            set => SetProperty(Model.Balance, value, Model, (m, v) => m.Balance = v);
        }

        [ObservableProperty]
        private bool showTransactions;

        [ObservableProperty]
        private int displayOrder;

        [ObservableProperty]
        private ObservableCollection<TransactionGroupViewModel> transactionGroups = [];

        // Update the transactions in the model and refresh the TransactionGroups collection
        public void UpdateTransactions(IEnumerable<Transaction> transactions)
        {
            Model.Transactions = transactions.ToList();
            RefreshTransactionGroups();
        }

        // Refresh the TransactionGroups collection based on the current transactions in the model
        private void RefreshTransactionGroups()
        {
            TransactionGroups = new ObservableCollection<TransactionGroupViewModel>(
                Model.Transactions
                    .GroupBy(t => t.Date.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g => new TransactionGroupViewModel(
                        g.Key,
                        g.OrderByDescending(t => t.Date)
                         .Select(t => new TransactionViewModel(t))
                    ))
            );
        }

        // Constructor for BankAccountViewModel
        public BankAccountViewModel(BankAccount model)
        {
            Model = model;
            RefreshTransactionGroups();
        }
    }
}