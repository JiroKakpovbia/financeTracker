using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using trackr.Models;

namespace trackr.Popups
{
    public partial class AddAccountPopup : Popup<BankAccount>
    {
        public AddAccountPopup()
        {
            InitializeComponent();
            BankPicker.ItemsSource = Enum.GetValues<BankInstitution>().Cast<object>().ToArray();
            TypePicker.ItemsSource = Enum.GetValues<AccountType>().Cast<object>().ToArray();

            BankPicker.SelectedIndex = -1;
            TypePicker.SelectedIndex = -1;
        }

        async void HandleAddAccountConfirmation(object sender, EventArgs e)
        {
            string accountName = NameEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(accountName) ||
                BankPicker.SelectedItem is not BankInstitution bankInstitution ||
                TypePicker.SelectedItem is not AccountType accountType)
            {
                await Shell.Current.DisplayAlertAsync("Error", "All fields are required. Please fill in all details to add an account.", "OK");
                return;
            }

            string bankInstitutionName = EnumDisplayNameConverter.GetDisplayName(bankInstitution);

            BankAccount account = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = accountName,
                BankInstitution = bankInstitutionName,
                Type = accountType,
                Balance = 0.00m,
            };

            await CloseAsync(account);
        }
    }
}
