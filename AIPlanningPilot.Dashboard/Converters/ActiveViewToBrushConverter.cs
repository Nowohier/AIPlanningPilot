using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Theme;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts an <see cref="ActiveView"/> value to a <see cref="Brush"/>.
/// Returns a gray background brush when the bound value matches the
/// <c>ConverterParameter</c> string; <see cref="Brushes.Transparent"/> otherwise.
/// Used for highlighting the active navigation button.
/// </summary>
public class ActiveViewToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ActiveView activeView && parameter is string paramString)
        {
            if (Enum.TryParse<ActiveView>(paramString, out var target))
            {
                return activeView == target ? DashboardBrushes.NotStartedPastelBrush : Brushes.Transparent;
            }
        }

        return Brushes.Transparent;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
