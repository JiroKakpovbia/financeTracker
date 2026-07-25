using trackr.Models;

namespace trackr.Views
{
    public partial class AddAccountView : ContentView
    {
        public class EnumItem<T> where T : Enum
        {
            public T EnumValue { get; init; }

            public string DisplayName =>
                EnumDisplayNameConverter.GetDisplayName(EnumValue);
        }

        public IEnumerable<EnumItem<BankInstitution>> BankInstitutions { get; } = Enum.GetValues<BankInstitution>().Select(b => new EnumItem<BankInstitution>
        {
            EnumValue = b
        }).ToList();

        public IEnumerable<EnumItem<AccountType>> AccountTypes { get; } = Enum.GetValues<AccountType>().Select(t => new EnumItem<AccountType>
        {
            EnumValue = t
        }).ToList();

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

        public EnumItem<BankInstitution>? SelectedBankItem
        {
            get => BankInstitutions.FirstOrDefault(x => x.EnumValue.Equals(SelectedBank));
            set => SelectedBank = value?.EnumValue;
        }

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

        public EnumItem<AccountType>? SelectedTypeItem
        {
            get => AccountTypes.FirstOrDefault(x => x.EnumValue.Equals(SelectedType));
            set => SelectedType = value?.EnumValue;
        }

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
            Console.WriteLine("Bank Institutions: " + string.Join(", ", BankInstitutions.Select(b => b.DisplayName)));
            Console.WriteLine("Account Types: " + string.Join(", ", AccountTypes.Select(t => t.DisplayName)));
        }

        public void Reset() // TODO: Values aren't reset, cannot create multiple accounts in one session
        {
            AccountName = string.Empty;
            SelectedBank = default;
            SelectedBankItem = null;
            SelectedType = default;
            SelectedTypeItem = null;
            Balance = default;
        }
    }
}
