using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace trackr.Models
{
    public enum AccountType
    {
        [Description("Visa Debit")]
        VISADebit,
        [Description("Visa Credit")]
        VISACredit,
        [Description("MasterCard Debit")]
        MasterCardDebit,
        [Description("MasterCard Credit")]
        MasterCardCredit,
        [Description("Savings")]
        Savings,
        [Description("High Interest Savings Account")]
        HISA,
        [Description("Other")]
        Other
    }

    public enum BankInstitution
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
        public string BankInstitution { get; set; } = string.Empty;

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
        public ObservableCollection<Transaction>? Transactions
        {
            get => _transactions;
            set
            {
                if (_transactions != value)
                {
                    _transactions = value;
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
        public int DisplayOrder {
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
        private ObservableCollection<Transaction>? _transactions;
        private bool _showTransactions;
        private int _displayOrder;
    }
}