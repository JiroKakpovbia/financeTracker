using System.Globalization;

namespace trackr
{
    public class AmountToColorConverter : IValueConverter
    {
        // Convert a decimal amount to a corresponding color based on its value
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var resources = Application.Current!.Resources;

            if (value is decimal amount)
            {
                if (amount < 0)
                    return (Color)resources["NegativeMoney"];

                if (amount > 0)
                    return (Color)resources["PositiveMoney"];
            }

            return (Color)resources["ZeroMoney"];
        }

        // ConvertBack is not implemented as this converter is intended for one-way binding 
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}