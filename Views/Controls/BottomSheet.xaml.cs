using System.ComponentModel;
using System.Windows.Input;

namespace trackr.Controls
{
    public partial class BottomSheet : ContentView
    {
        private View? trackedContent;

        // Bindable property for the content of the BottomSheet
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

        // Bindable property for the title of the BottomSheet
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

        // Bindable property for the close command of the BottomSheet
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

        // Constructor for BottomSheet
        public BottomSheet()
        {
            InitializeComponent();

            ContentHost.SizeChanged += OnContentHostSizeChanged;
        }

        // Callback method to handle changes in the SheetContent property
        private static void OnSheetContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BottomSheet sheet)
                sheet.SetTrackedContent(oldValue as View, newValue as View);
        }

        private void SetTrackedContent(View? oldContent, View? newContent)
        {
            if (oldContent != null)
            {
                oldContent.SizeChanged -= OnTrackedContentSizeChanged;
                oldContent.PropertyChanged -= OnTrackedContentPropertyChanged;
            }

            trackedContent = newContent;
            ContentHost.Content = newContent;

            if (newContent != null)
            {
                newContent.SizeChanged += OnTrackedContentSizeChanged;
                newContent.PropertyChanged += OnTrackedContentPropertyChanged;
            }

            RequestRelayout();
        }

        private void OnContentHostSizeChanged(object? sender, EventArgs e)
        {
            RequestRelayout();
        }

        private void OnTrackedContentSizeChanged(object? sender, EventArgs e)
        {
            RequestRelayout();
        }

        private void OnTrackedContentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisualElement.IsVisible) ||
                e.PropertyName == nameof(VisualElement.HeightRequest) ||
                e.PropertyName == nameof(VisualElement.MinimumHeightRequest) ||
                e.PropertyName == nameof(VisualElement.MaximumHeightRequest))
            {
                RequestRelayout();
            }
        }

        private void RequestRelayout()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ApplyRelayoutPass();

                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1), ApplyRelayoutPass);
            });
        }

        private void ApplyRelayoutPass()
        {
            ContentHost.HeightRequest = -1;
            SheetScrollView.HeightRequest = -1;
            Sheet.HeightRequest = -1;

            InvalidateElement(trackedContent);
            InvalidateElement(ContentHost);
            InvalidateElement(SheetScrollView);
            InvalidateElement(Sheet);
            InvalidateElement(this);
        }

        private static void InvalidateElement(VisualElement? element)
        {
            if (element == null)
                return;

            element.InvalidateMeasure();

            if (element.Parent is VisualElement parent)
                parent.InvalidateMeasure();
        }

        // Show the BottomSheet with an animation
        public async Task Show()
        {
            IsVisible = true;

            await Task.Yield();

            RequestRelayout();

            Sheet.TranslationY = Sheet.Height;

            await Sheet.TranslateToAsync(
                0,
                0,
                300,
                Easing.SpringOut);
        }

        // Hide the BottomSheet with an animation
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