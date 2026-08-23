using System.Globalization;
using trackr.Models;

namespace trackr
{
    public class LastImportConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ImportBatch importBatch && importBatch.Id > 0) return $"Last Import: {importBatch.ImportedAt.ToLocalTime():MMM d, yyyy} at {importBatch.ImportedAt.ToLocalTime():h:mm tt}";

            return "Last Import: Never";
        }

        // ConvertBack is not implemented as this converter is intended for one-way binding 
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}