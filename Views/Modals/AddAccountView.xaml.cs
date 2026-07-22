using CommunityToolkit.Maui.Views;
using trackr.Models;

namespace trackr.Views
{
    public partial class AddAccountView : ContentView
    {
        public event EventHandler<BankAccount>? AddAccountClicked;

        public AddAccountView()
        {
            InitializeComponent();
            
            BankPicker.ItemsSource = Enum.GetValues<AccountBankInstitution>().Cast<object>().ToArray();
            TypePicker.ItemsSource = Enum.GetValues<AccountType>().Cast<object>().ToArray();

            Reset();
        }

        public void Reset()
        {
            NameEntry.Text = string.Empty;
            BalanceEntry.Text = string.Empty;

            BankPicker.SelectedIndex = -1;
            TypePicker.SelectedIndex = -1;
        }

        private async void HandleAddAccountConfirmation(object sender, EventArgs e)
        {
            Console.WriteLine("Add Account confirmation button clicked.");

            try
            {
                string accountName = NameEntry.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(accountName) ||
                    BankPicker.SelectedItem is not AccountBankInstitution bankInstitution ||
                    TypePicker.SelectedItem is not AccountType accountType)
                { // TODO: Fix popup for empty fields
                    Console.WriteLine("Validation failed: One or more fields are empty.");
                    await Application.Current.MainPage.DisplayAlertAsync("Error", "All fields are required. Please fill in all details to add an account.", "OK");
                    return;
                }

                BankAccount account = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = accountName,
                    BankInstitution = bankInstitution,
                    Type = accountType,
                    Balance = decimal.TryParse(
                        BalanceEntry.Text,
                        out decimal balance)
                        ? balance
                        : 0.00m,
                };

                AddAccountClicked?.Invoke(this, account);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleAddAccountConfirmation: {ex.Message}\n");
            }
        }
    }
}
