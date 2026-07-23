using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace trackr.Models
{
    public enum AccountType
    {
        [Description("Chequing")]
        Chequing,
        [Description("Savings")]
        Savings,
        [Description("Credit Card")]
        CreditCard,
        [Description("Cash")]
        Cash,
        // [Description("Investment")]
        // Investment,
        // [Description("Loan")]
        // Loan,
        // [Description("Mortgage")]
        // Mortgage,
        // [Description("Line Of Credit")]
        // LineOfCredit,
        [Description("Other")]
        Other
    }

    public enum AccountBankInstitution
    {
        [Description("Capital One")]
        CapitalOne,
        [Description("CIBC")]
        CIBC,
        [Description("RBC")]
        RBC,
        [Description("TD")]
        TD
    }

    [Table("BankAccounts")]
    public class BankAccount : INotifyPropertyChanged
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string Name { get; set; } = string.Empty;

        [Indexed]
        public AccountBankInstitution BankInstitution { get; set; }

        public AccountType Type { get; set; }

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged();
                }
            }
        }

        [Ignore]
        public ObservableCollection<TransactionGroup> TransactionGroups
        {
            get => _transactionGroups;
            set
            {
                if (_transactionGroups != value)
                {
                    _transactionGroups = value;
                    OnPropertyChanged();
                }
            }
        }

        [Ignore]
        public bool ShowTransactions
        {
            get => _showTransactions;
            set
            {
                if (_showTransactions != value)
                {
                    _showTransactions = value;
                    OnPropertyChanged();
                }
            }
        }

        [Ignore]
        public int DisplayOrder
        {
            get => _displayOrder;
            set
            {
                if (_displayOrder != value)
                {
                    _displayOrder = value;
                    OnPropertyChanged();
                }
            }
        } // Optional: For UI display order, if needed

        // Implement INotifyPropertyChanged to notify the UI of property changes
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private decimal _balance;
        private ObservableCollection<TransactionGroup>? _transactionGroups;
        private bool _showTransactions;
        private int _displayOrder;
    }
}