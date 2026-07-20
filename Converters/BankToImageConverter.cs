using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace trackr
{
    public class BankToImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string bank = value switch
            {
                string stringValue => stringValue,
                Enum enumValue => EnumDisplayNameConverter.GetDisplayName(enumValue),
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(bank))
            {
                return string.Empty;
            }

            return $"{bank.ToLower().Replace(" ", string.Empty)}.png";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
