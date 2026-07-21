using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace trackr
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount < 0 ? Colors.Red : amount > 0 ? Colors.Green : Colors.Gray;
            }
            return Colors.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}