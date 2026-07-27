using System.Windows.Input;

namespace trackr.Components;

public partial class AccountCard : ContentView
{
    // Bindable property for the ShowAccountOptionsCommand
    public static readonly BindableProperty ShowAccountOptionsCommandProperty =
        BindableProperty.Create(
            nameof(ShowAccountOptionsCommand),
            typeof(ICommand),
            typeof(AccountCard));

    public ICommand? ShowAccountOptionsCommand
    {
        get => (ICommand?)GetValue(ShowAccountOptionsCommandProperty);
        set => SetValue(ShowAccountOptionsCommandProperty, value);
    }

    // Bindable property for the LogoTapCommand
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
        
    // Bindable property for the ToggleCommand
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

    // Constructor for AccountCard
    public AccountCard()
    {
        InitializeComponent();
    }
}