using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace trackr.Models
{
    [Table("BankAccounts")]
    public class BankAccount : INotifyPropertyChanged
    {
        private string _name;
        private string _id;
        private string _bank;
        private string _type;
        private decimal _balance;
        private ObservableCollection<Transaction> _transactions;
        private bool _showTransactions;

        public BankAccount()
        {
            _name = string.Empty;
            _id = string.Empty;
            _bank = string.Empty;
            _type = string.Empty;
            _balance = 0.00m;
            _transactions = new ObservableCollection<Transaction>();
            ShowTransactions = false;
        }

        [PrimaryKey]
        public string Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        [Indexed]
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }


        [Indexed]
        public string Bank
        {
            get => _bank;
            set { if (_bank != value) { _bank = value; OnPropertyChanged(); } }
        }

        public string Type
        {
            get => _type;
            set { if (_type != value) { _type = value; OnPropertyChanged(); } }
        }

        public decimal Balance
        {
            get => _balance;
            set { if (_balance != value) { _balance = value; OnPropertyChanged(); } }
        }

        [Ignore]
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set { if (_transactions != value) { _transactions = value; OnPropertyChanged(); } }
        }

        public bool ShowTransactions
        {
            get => _showTransactions;
            set { if (_showTransactions != value) { _showTransactions = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}