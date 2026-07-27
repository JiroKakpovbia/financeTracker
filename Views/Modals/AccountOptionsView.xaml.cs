using trackr.ViewModels;

namespace trackr.Views
{
    public partial class AccountOptionsView : ContentView
    {
        // Bindable property for SelectedAccount
        private static readonly BindableProperty SelectedAccountProperty =
            BindableProperty.Create(
                nameof(SelectedAccount),
                typeof(BankAccountViewModel),
                typeof(AccountOptionsView));

        public BankAccountViewModel SelectedAccount
        {
            get => (BankAccountViewModel)GetValue(SelectedAccountProperty);
            set => SetValue(SelectedAccountProperty, value);
        }

        // Constructor for AccountOptionsView
        public AccountOptionsView()
        {
            InitializeComponent();
        }
    }
}
