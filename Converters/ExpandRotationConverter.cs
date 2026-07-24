using System.Globalization;

namespace trackr
{

    public class ExpandRotationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showTransactions)
                return showTransactions ? 180.0 : 0.0;


            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double rotation)
                return rotation == 180.0;


            return false;
        }
    }
}