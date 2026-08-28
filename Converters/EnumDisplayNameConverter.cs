using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using trackr.Models;

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
            {
                return parameter?.ToString()?.ToLowerInvariant() switch
                {
                    "short" => GetShortName(enumValue),
                    "long" => GetLongName(enumValue),
                    _ => GetDisplayName(enumValue),
                };
            }

            return string.Empty;
        }

        // ConvertBack is not implemented as this converter is intended for one-way binding
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        // Get the display name of an enum value using the DescriptionAttribute if available
        public static string GetDisplayName(Enum value)
        {
            MemberInfo? memberInfo = GetMemberInfo(value);

            if (memberInfo is not null)
            {
                DescriptionAttribute? description =
                    memberInfo.GetCustomAttribute<DescriptionAttribute>();

                if (description is not null)
                    return description.Description;
            }

            return value.ToString();
        }

        // Get the short name of an enum value using the BankInstitutionInfoAttribute if available, otherwise fallback to the display name
        public static string GetShortName(Enum value)
        {
            MemberInfo? memberInfo = GetMemberInfo(value);

            return memberInfo?
                .GetCustomAttribute<BankInstitutionInfoAttribute>()?
                .ShortName
                ?? GetDisplayName(value);
        }

        // Get the long name of an enum value using the BankInstitutionInfoAttribute if available, otherwise fallback to the display name
        public static string GetLongName(Enum value)
        {
            MemberInfo? memberInfo = GetMemberInfo(value);

            return memberInfo?
                .GetCustomAttribute<BankInstitutionInfoAttribute>()?
                .LongName
                ?? GetDisplayName(value);
        }

        // Helper method to retrieve the MemberInfo for an enum value
        private static MemberInfo? GetMemberInfo(Enum value)
        {
            return value
                .GetType()
                .GetMember(value.ToString())
                .FirstOrDefault();
        }
    }
}