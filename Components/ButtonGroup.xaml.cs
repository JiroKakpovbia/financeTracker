using System.Windows.Input;

namespace trackr.Components;

public partial class ButtonGroup : ContentView
{
    // Bindable property for the PrimaryButtonText
    public static readonly BindableProperty PrimaryButtonTextProperty =
        BindableProperty.Create(
            nameof(PrimaryButtonText),
            typeof(string),
            typeof(ButtonGroup),
            default(string));

    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    // Bindable property for the PrimaryButtonCommand
    public static readonly BindableProperty PrimaryButtonCommandProperty =
        BindableProperty.Create(
            nameof(PrimaryButtonCommand),
            typeof(ICommand),
            typeof(ButtonGroup),
            default(ICommand));

    public ICommand? PrimaryButtonCommand
    {
        get => (ICommand?)GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    // Bindable property for the SecondaryButtonText
    public static readonly BindableProperty SecondaryButtonTextProperty =
        BindableProperty.Create(
            nameof(SecondaryButtonText),
            typeof(string),
            typeof(ButtonGroup),
            default(string));

    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    // Bindable property for the SecondaryButtonCommand
    public static readonly BindableProperty SecondaryButtonCommandProperty =
        BindableProperty.Create(
            nameof(SecondaryButtonCommand),
            typeof(ICommand),
            typeof(ButtonGroup),
            default(ICommand));

    public ICommand? SecondaryButtonCommand
    {
        get => (ICommand?)GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    // Constructor for ButtonGroup
    public ButtonGroup()
    {
        InitializeComponent();
    }
}