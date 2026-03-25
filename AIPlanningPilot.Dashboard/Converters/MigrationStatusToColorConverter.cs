using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a <see cref="MigrationStatus"/> to a <see cref="System.Windows.Media.SolidColorBrush"/>
/// for visual status indication in the migration tracker.
/// </summary>
public class MigrationStatusToColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MigrationStatus status)
        {
            return status switch
            {
                MigrationStatus.Done => DashboardBrushes.GreenBrush,
                MigrationStatus.InProgress => DashboardBrushes.AmberBrush,
                MigrationStatus.Skipped => DashboardBrushes.RedBrush,
                MigrationStatus.NotStarted => DashboardBrushes.GrayBrush,
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
