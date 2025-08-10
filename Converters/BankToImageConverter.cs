using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace financeTracker
{
    public class BankToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string bank)
            {
                return $"{bank.ToLower().Replace(" ", "")}.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
