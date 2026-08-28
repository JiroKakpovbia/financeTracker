namespace trackr.Components;

public partial class TransactionCard : ContentView
{
    public static readonly BindableProperty CardPaddingProperty =
        BindableProperty.Create(
            nameof(CardPadding),
            typeof(Thickness),
            typeof(TransactionCard),
            new Thickness(10));

    public Thickness CardPadding
    {
        get => (Thickness)GetValue(CardPaddingProperty);
        set => SetValue(CardPaddingProperty, value);
    }

    public static readonly BindableProperty ShowMoreInfoProperty =
        BindableProperty.Create(
            nameof(ShowMoreInfo),
            typeof(bool),
            typeof(TransactionCard),
            true);

    public bool ShowMoreInfo
    {
        get => (bool)GetValue(ShowMoreInfoProperty);
        set => SetValue(ShowMoreInfoProperty, value);
    }

    // Constructor for TransactionCard`
    public TransactionCard()
    {
        InitializeComponent();
    }
}