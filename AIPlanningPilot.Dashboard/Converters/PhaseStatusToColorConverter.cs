using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a phase status string to a <see cref="System.Windows.Media.SolidColorBrush"/>.
/// Uses the same color palette as <see cref="ActionStatusToColorConverter"/>:
/// Green for done/complete, Blue for in progress, Gray for not started.
/// <para>Parameters:</para>
/// <list type="bullet">
///   <item><c>"Foreground"</c> - returns text color (white for colored badges, gray for not started)</item>
///   <item><c>"Badge"</c> - returns Transparent background for "not started" (no visible badge)</item>
///   <item>No parameter - returns GrayBrush background for "not started" (visible circle/indicator)</item>
/// </list>
/// </summary>
public class PhaseStatusToColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = PhaseStatusClassifier.Classify(value as string);
        var param = (parameter as string ?? "").ToLowerInvariant();

        return category switch
        {
            PhaseStatusCategory.Done =>
                param == "foreground" ? DashboardBrushes.WhiteBrush : DashboardBrushes.GreenBrush,
            PhaseStatusCategory.InProgress =>
                param == "foreground" ? DashboardBrushes.WhiteBrush : DashboardBrushes.BlueBrush,
            _ => param switch
            {
                "foreground" => DashboardBrushes.GrayTextBrush,
                "badge" => DashboardBrushes.TransparentBrush,
                _ => DashboardBrushes.LightGrayBrush
            }
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
