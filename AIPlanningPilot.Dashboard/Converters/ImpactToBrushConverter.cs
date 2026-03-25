using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts an <see cref="ImpactLevel"/> to a <see cref="System.Windows.Media.SolidColorBrush"/>
/// for visual urgency indication.
/// Pass <c>"Foreground"</c> as <see cref="IValueConverter.Convert"/> parameter
/// to get the appropriate text color; otherwise the background color is returned.
/// </summary>
public class ImpactToBrushConverter : IValueConverter
{
    private const string ForegroundParameter = "Foreground";

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isForeground = ForegroundParameter.Equals(parameter as string, StringComparison.OrdinalIgnoreCase);

        if (value is ImpactLevel impact)
        {
            return impact switch
            {
                ImpactLevel.VeryHigh => isForeground ? DashboardBrushes.WhiteBrush : DashboardBrushes.VeryHighImpactBrush,
                ImpactLevel.High => isForeground ? DashboardBrushes.WhiteBrush : DashboardBrushes.HighImpactBrush,
                ImpactLevel.Medium => isForeground ? DashboardBrushes.DarkForegroundBrush : DashboardBrushes.MediumImpactBrush,
                ImpactLevel.Low => isForeground ? DashboardBrushes.WhiteBrush : DashboardBrushes.LowImpactBrush,
                _ => isForeground ? DashboardBrushes.WhiteBrush : DashboardBrushes.GrayBrush
            };
        }

        return isForeground ? DashboardBrushes.WhiteBrush : DashboardBrushes.GrayBrush;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
