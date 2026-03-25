using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a <see cref="double"/> percentage (0-100) to a <see cref="GridLength"/>
/// with star sizing for use in segmented progress bars.
/// </summary>
public class DoubleToGridLengthConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d and > 0)
        {
            return new GridLength(d, GridUnitType.Star);
        }
        return new GridLength(0, GridUnitType.Pixel);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
