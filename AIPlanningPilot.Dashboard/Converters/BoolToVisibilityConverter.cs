using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a boolean value to a <see cref="Visibility"/> value.
/// <c>true</c> maps to <see cref="Visibility.Visible"/>, <c>false</c> to <see cref="Visibility.Collapsed"/>.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
