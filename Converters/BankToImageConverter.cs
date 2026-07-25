using System.Globalization;

namespace trackr
{
    public class BankToImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string bankImage = value?.ToString()?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(bankImage))
                return "bank_default.png"; // Return a default image if the bank value is null or empty

            return $"{bankImage}.png";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
