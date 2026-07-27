using System.Globalization;

namespace trackr
{
    public class BankToImageConverter : IValueConverter
    {
        // Convert a bank name to its corresponding image file name
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string bankImage = value?.ToString()?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(bankImage))
                return "bank_default.png"; // Return a default image if the bank value is null or empty

            return $"{bankImage}.png";
        }

        // ConvertBack is not implemented as this converter is intended for one-way binding 
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
