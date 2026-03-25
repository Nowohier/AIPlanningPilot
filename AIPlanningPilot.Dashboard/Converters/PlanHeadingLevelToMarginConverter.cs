using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a plan heading level (int) to a <see cref="Thickness"/> margin.
/// Sub-headings (level > 2) are indented with a left margin.
/// </summary>
public class PlanHeadingLevelToMarginConverter : IValueConverter
{
    private static readonly Thickness DefaultMargin = new(0, 2, 0, 2);
    private static readonly Thickness IndentedMargin = new(16, 2, 0, 2);

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int level && level > 2)
        {
            return IndentedMargin;
        }
        return DefaultMargin;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
