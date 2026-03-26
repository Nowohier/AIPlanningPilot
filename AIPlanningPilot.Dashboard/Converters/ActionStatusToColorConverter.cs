using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Theme;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts an <see cref="ActionStatus"/> to a <see cref="System.Windows.Media.SolidColorBrush"/>
/// for visual status indication in the dashboard.
/// </summary>
public class ActionStatusToColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ActionStatus status)
        {
            return status switch
            {
                ActionStatus.Done => DashboardBrushes.GreenBrush,
                ActionStatus.Next => DashboardBrushes.BlueBrush,
                ActionStatus.Pending => DashboardBrushes.GrayBrush,
                _ => DashboardBrushes.GrayBrush
            };
        }
        return DashboardBrushes.GrayBrush;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
