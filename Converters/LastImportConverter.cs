using System.Globalization;
using trackr.ViewModels;

namespace trackr
{
    public class LastImportConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ImportBatchViewModel importBatch) return $"Last Import: {importBatch.ImportedAt.ToLocalTime():MMM d, yyyy} at {importBatch.ImportedAt.ToLocalTime():h:mm tt}";

            return "Last Import: Never";
        }

        // ConvertBack is not implemented as this converter is intended for one-way binding 
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

    }
}