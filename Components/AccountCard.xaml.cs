using System.Windows.Input;

namespace trackr.Components;

public partial class AccountCard : ContentView
{
    public AccountCard()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ShowMenuCommandProperty =
        BindableProperty.Create(
            nameof(ShowMenuCommand),
            typeof(ICommand),
            typeof(AccountCard));

    public ICommand? ShowMenuCommand
    {
        get => (ICommand?)GetValue(ShowMenuCommandProperty);
        set => SetValue(ShowMenuCommandProperty, value);
    }

    public static readonly BindableProperty LogoTapCommandProperty =
        BindableProperty.Create(
            nameof(LogoTapCommand),
            typeof(ICommand),
            typeof(AccountCard));

    public ICommand? LogoTapCommand
    {
        get => (ICommand?)GetValue(LogoTapCommandProperty);
        set => SetValue(LogoTapCommandProperty, value);
    }

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(
            nameof(ToggleCommand),
            typeof(ICommand),
            typeof(AccountCard));

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }
}