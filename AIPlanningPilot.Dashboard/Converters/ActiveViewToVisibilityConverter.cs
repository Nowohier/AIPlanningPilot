using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts an <see cref="ActiveView"/> value to <see cref="Visibility"/>.
/// Returns <see cref="Visibility.Visible"/> when the bound value matches the
/// <c>ConverterParameter</c> string; <see cref="Visibility.Collapsed"/> otherwise.
/// </summary>
public class ActiveViewToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ActiveView activeView && parameter is string paramString)
        {
            if (Enum.TryParse<ActiveView>(paramString, out var target))
            {
                return activeView == target ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
