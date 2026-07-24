using trackr.Models;

namespace trackr.Views
{
    public partial class AddAccountView : ContentView
    {
        public IEnumerable<BankInstitution> BankInstitutions { get; } = Enum.GetValues<BankInstitution>();

        public IEnumerable<AccountType> AccountTypes { get; } = Enum.GetValues<AccountType>();

        public static readonly BindableProperty AccountNameProperty =
    BindableProperty.Create(
        nameof(AccountName),
        typeof(string),
        typeof(AddAccountView));

        public string AccountName
        {
            get => (string)GetValue(AccountNameProperty);
            set => SetValue(AccountNameProperty, value);
        }

        public static readonly BindableProperty SelectedBankProperty =
            BindableProperty.Create(
                nameof(SelectedBank),
                typeof(BankInstitution?),
                typeof(AddAccountView));

        public BankInstitution? SelectedBank
        {
            get => (BankInstitution?)GetValue(SelectedBankProperty);
            set => SetValue(SelectedBankProperty, value);
        }

        public static readonly BindableProperty SelectedTypeProperty =
            BindableProperty.Create(
                nameof(SelectedType),
                typeof(AccountType?),
                typeof(AddAccountView));

        public AccountType? SelectedType
        {
            get => (AccountType?)GetValue(SelectedTypeProperty);
            set => SetValue(SelectedTypeProperty, value);
        }

        public static readonly BindableProperty BalanceProperty =
            BindableProperty.Create(
                nameof(Balance),
                typeof(decimal),
                typeof(AddAccountView));

        public decimal Balance
        {
            get => (decimal)GetValue(BalanceProperty);
            set => SetValue(BalanceProperty, value);
        }
        public AddAccountView()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            AccountName = string.Empty;
            SelectedBank = default;
            SelectedType = default;
            Balance = default;
        }
    }
}
