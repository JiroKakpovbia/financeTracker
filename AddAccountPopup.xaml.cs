using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;

namespace financeTracker;

public partial class AddAccountPopup : Popup
{
    public AddAccountPopup()
    {
        InitializeComponent();
    }

    void OnAddAccountConfirmClicked(object sender, EventArgs e)
    {

        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            BankPicker.SelectedItem == null ||
            TypePicker.SelectedItem == null)
        {
            Close(null);
            return;
        }

        string name = NameEntry.Text.Trim();
        string bank = (BankPicker.SelectedItem as string)!;
        string type = (TypePicker.SelectedItem as string)!;

        var account = new BankAccount
        {
            Name = name,
            Id = $"{bank}-{type}-{name}",
            Bank = bank,
            Type = type,
            Balance = 0.00m,
            Transactions = new ObservableCollection<Transaction>(),
            ShowTransactions = false
        };

        Close(account);
    }
}
