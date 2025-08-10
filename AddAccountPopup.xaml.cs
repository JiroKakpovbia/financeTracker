using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;

namespace financeTracker;

public partial class AddAccountPopup : Popup
{
    public AddAccountPopup()
    {
        InitializeComponent();
    }

    async void HandleAddAccountConfirmation(object sender, EventArgs e)
    {
        
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(BankPicker.SelectedItem as string) ||
            string.IsNullOrWhiteSpace(TypePicker.SelectedItem as string))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "All fields are required. Please fill in all details to add an account.", "OK");
            return;
        }

        string name = NameEntry.Text.Trim();
        string bank = BankPicker.SelectedItem as string;
        string type = TypePicker.SelectedItem as string;


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
