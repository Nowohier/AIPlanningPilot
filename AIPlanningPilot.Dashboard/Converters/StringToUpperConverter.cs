using System.Globalization;
using System.Windows.Data;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a string value to its uppercase representation.
/// </summary>
public class StringToUpperConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string s ? s.ToUpperInvariant() : value;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
