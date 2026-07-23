using CommunityToolkit.Maui.Views;
using trackr.Models;

namespace trackr.Views
{
    public partial class AccountOptionsView : ContentView
    {
        public BankAccount? Account { get; set; }

        public event EventHandler<BankAccount>? RenameAccountClicked;
        public event EventHandler<BankAccount>? ImportCSVClicked;
        public event EventHandler<BankAccount>? MoveAccountClicked;
        public event EventHandler<BankAccount>? DeleteAccountClicked;

        public AccountOptionsView()
        {
            InitializeComponent();
        }

        public void SetAccount(BankAccount account)
        {
            Account = account;
        }

        private void HandleRenameAccountConfirmation(object sender, EventArgs e)
        {
            if (Account != null)
                RenameAccountClicked?.Invoke(this, Account);
        }

        private void HandleImportCSVConfirmation(object sender, EventArgs e)
        {
            if (Account != null)
                ImportCSVClicked?.Invoke(this, Account);
        }

        private void HandleMoveAccountConfirmation(object sender, EventArgs e)
        {
            if (Account != null)
                MoveAccountClicked?.Invoke(this, Account);
        }

        private void HandleDeleteAccountConfirmation(object sender, EventArgs e)
        {
            if (Account != null)
                DeleteAccountClicked?.Invoke(this, Account);
        }
    }
}
