using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace financeTracker
{
    public class BankToImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string bank)
            {
                return $"{bank.ToLower().Replace(" ", "")}.png";
            }
            throw new ArgumentException("Value must be a string representing the bank name.", nameof(value));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
