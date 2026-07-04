using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace trackr
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return GetDisplayName(enumValue);
            }

            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public static string GetDisplayName(Enum value)
        {
            MemberInfo[] memberInfo = value.GetType().GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                DescriptionAttribute? descriptionAttribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }

            return value.ToString();
        }
    }
}