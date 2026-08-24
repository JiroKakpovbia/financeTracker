using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace trackr
{
    public class CategoryDisplayNameConverter : IValueConverter
    {
        // Convert an enum value to its display name or string representation
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return "Uncategorized";

            return value;
        }

        // ConvertBack is not implemented as this converter is intended for one-way binding
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}