using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Theme;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a <see cref="ComplexityTier"/> to a <see cref="System.Windows.Media.SolidColorBrush"/>
/// for visual complexity indication in the migration tracker.
/// </summary>
public class ComplexityTierToColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ComplexityTier tier)
        {
            return tier switch
            {
                ComplexityTier.Simple => DashboardBrushes.BlueBrush,
                ComplexityTier.Medium => DashboardBrushes.AmberBrush,
                ComplexityTier.Complex => DashboardBrushes.OrangeBrush,
                ComplexityTier.VeryComplex => DashboardBrushes.RedBrush,
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
