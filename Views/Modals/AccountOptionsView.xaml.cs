using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using trackr.Models;

namespace trackr.Views
{
    public partial class AccountOptionsView : ContentView
    {
        public static readonly BindableProperty SelectedAccountProperty =
            BindableProperty.Create(
                nameof(SelectedAccount),
                typeof(BankAccount),
                typeof(AccountOptionsView));

        public BankAccount SelectedAccount
        {
            get => (BankAccount)GetValue(SelectedAccountProperty);
            set => SetValue(SelectedAccountProperty, value);
        }

        public AccountOptionsView()
        {
            InitializeComponent();
        }
    }
}
