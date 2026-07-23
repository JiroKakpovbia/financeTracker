using System.Globalization;

namespace trackr
{
    public class AmountToColorConverter : IValueConverter
    {
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

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}