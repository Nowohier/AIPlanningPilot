using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a potentially null value to a <see cref="Visibility"/> value.
/// Non-null maps to <see cref="Visibility.Visible"/>, null to <see cref="Visibility.Collapsed"/>.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
