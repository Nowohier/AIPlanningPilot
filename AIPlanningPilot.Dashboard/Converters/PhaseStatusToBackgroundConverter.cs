using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Theme;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a phase status string to a pastel background <see cref="System.Windows.Media.SolidColorBrush"/>
/// for phase progress cards. Uses lighter tints of the status colors.
/// </summary>
public class PhaseStatusToBackgroundConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = PhaseStatusClassifier.Classify(value as string);

        return category switch
        {
            PhaseStatusCategory.Done => DashboardBrushes.DonePastelBrush,
            PhaseStatusCategory.InProgress => DashboardBrushes.InProgressPastelBrush,
            _ => DashboardBrushes.NotStartedPastelBrush
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
