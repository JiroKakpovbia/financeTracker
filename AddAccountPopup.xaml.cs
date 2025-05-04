using CommunityToolkit.Maui.Views;

namespace financeTracker;

public partial class AddAccountPopup : Popup
{
    public AddAccountPopup()
    {
        InitializeComponent();
    }

    void OnAddAccountConfirmClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text.Trim();
		string bank = BankPicker.SelectedItem as string;
        string type = TypePicker.SelectedItem as string;

		if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            BankPicker.SelectedItem == null ||
            TypePicker.SelectedItem == null)
        {
            Close(null);
            return;
        }

		var account = new BankAccount
		{
			Name = name,
			Bank = bank,
			Type = type,
			Transactions = new List<Transaction>()
		};

        Close(account);
    }
}
