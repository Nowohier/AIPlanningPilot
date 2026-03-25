using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a boolean value to an inverted <see cref="Visibility"/> value.
/// <c>true</c> maps to <see cref="Visibility.Collapsed"/>, <c>false</c> to <see cref="Visibility.Visible"/>.
/// Used to show loading indicators when data has not yet been loaded.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
