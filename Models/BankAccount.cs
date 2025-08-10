using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class BankAccount : INotifyPropertyChanged
{
    private string _name;
    private string _id;
    private string _bank;
    private string _type;
    private decimal _balance;
    private ObservableCollection<Transaction> _transactions = new();
    private bool _showTransactions;

    public string Name
    {
        get => _name;
        set { if (_name != value) { _name = value; OnPropertyChanged(); } }
    }

    public string Id
    {
        get => _id;
        set { if (_id != value) { _id = value; OnPropertyChanged(); } }
    }

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
