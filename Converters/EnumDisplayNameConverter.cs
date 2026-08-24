using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace trackr
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        // Convert an enum value to its display name or string representation
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
                return stringValue;

            if (value is Enum enumValue)
                return GetDisplayName(enumValue);

            return string.Empty;
        }
 
        // ConvertBack is not implemented as this converter is intended for one-way binding
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        // Get the display name of an enum value using the DescriptionAttribute if available
        public static string GetDisplayName(Enum value)
        {
            MemberInfo[] memberInfo = value.GetType().GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                DescriptionAttribute? descriptionAttribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                    return descriptionAttribute.Description;
            }

            return value.ToString();
        }
    }
}