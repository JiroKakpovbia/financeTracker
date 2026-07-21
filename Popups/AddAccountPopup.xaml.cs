using CommunityToolkit.Maui.Views;
using trackr.Models;

namespace trackr.Popups
{
    public partial class AddAccountPopup : Popup<BankAccount>
    {
        public AddAccountPopup()
        {
            InitializeComponent();
            BankPicker.ItemsSource = Enum.GetValues<AccountBankInstitution>().Cast<object>().ToArray();
            TypePicker.ItemsSource = Enum.GetValues<AccountType>().Cast<object>().ToArray();

            BankPicker.SelectedIndex = -1;
            TypePicker.SelectedIndex = -1;
        }

        async void HandleAddAccountConfirmation(object sender, EventArgs e)
        {
            Console.WriteLine("Add Account confirmation button clicked.");

            try
            {
                string accountName = NameEntry.Text?.Trim() ?? string.Empty;

                Console.WriteLine($"Account Name: {accountName}");
                Console.WriteLine($"Selected Bank Institution: {BankPicker.SelectedItem}");
                Console.WriteLine($"Selected Account Type: {TypePicker.SelectedItem}");

                if (string.IsNullOrWhiteSpace(accountName) ||
                    BankPicker.SelectedItem is not AccountBankInstitution bankInstitution ||
                    TypePicker.SelectedItem is not AccountType accountType)
                { // TODO: Fix popup for empty fields
                    Console.WriteLine("Validation failed: One or more fields are empty.");
                    await Application.Current.MainPage.DisplayAlertAsync("Error", "All fields are required. Please fill in all details to add an account.", "OK");
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleAddAccountConfirmation: {ex.Message}\n");
            }
        }
    }
}
