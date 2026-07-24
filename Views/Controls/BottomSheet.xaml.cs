using System.Windows.Input;

namespace trackr.Controls
{
    public partial class BottomSheet : ContentView
    {
        public static readonly BindableProperty SheetContentProperty =
                BindableProperty.Create(
                    nameof(SheetContent),
                    typeof(View),
                    typeof(BottomSheet),
                    propertyChanged: OnSheetContentChanged);

        public View SheetContent
        {
            get => (View)GetValue(SheetContentProperty);
            set => SetValue(SheetContentProperty, value);
        }

        public BottomSheet()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                nameof(Title),
                typeof(string),
                typeof(BottomSheet),
                string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        
        public static readonly BindableProperty CloseCommandProperty =
        BindableProperty.Create(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(BottomSheet));

        public ICommand? CloseCommand
        {
            get => (ICommand?)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        private static void OnSheetContentChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
        {
            if (bindable is BottomSheet sheet)
                sheet.ContentHost.Content = newValue as View;
        }

        public async Task Show()
        {
            IsVisible = true;

            await Task.Yield();

            Sheet.TranslationY = Sheet.Height;

            await Sheet.TranslateToAsync(
                0,
                0,
                300,
                Easing.SpringOut);
        }

        public async Task Hide()
        {
            await Sheet.TranslateToAsync(
                0,
                Sheet.Height,
                250,
                Easing.CubicIn);

            IsVisible = false;
        }
    }
}