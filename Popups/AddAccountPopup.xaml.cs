using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using trackr.Models;

namespace trackr.Popups
{
    public partial class AddAccountPopup : Popup
    {
        public AddAccountPopup()
        {
            InitializeComponent();
        }

        async void HandleAddAccountConfirmation(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                string.IsNullOrWhiteSpace(BankPicker.SelectedItem?.ToString()) ||
                string.IsNullOrWhiteSpace(TypePicker.SelectedItem?.ToString()))
            {
                await Shell.Current.DisplayAlert("Error", "All fields are required. Please fill in all details to add an account.", "OK");
                return;
            }

            string name = NameEntry.Text.Trim();
            string bank = BankPicker.SelectedItem?.ToString()!;
            string type = TypePicker.SelectedItem?.ToString()!;

            BankAccount account = new BankAccount
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                Bank = bank,
                Type = type,
            };

            Close(account);
        }
    }
}
