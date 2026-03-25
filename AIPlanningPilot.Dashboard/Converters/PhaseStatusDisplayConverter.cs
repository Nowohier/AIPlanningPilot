using System.Globalization;
using System.Windows.Data;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts phase status text for display in the phase list.
/// Normalizes partial progress statuses (e.g. "Day 1 done") to "In Progress".
/// </summary>
public class PhaseStatusDisplayConverter : IValueConverter
{
    private const string InProgressDisplay = "In Progress";
    private const string DoneDisplay = "Done";
    private const string NotStartedDisplay = "Not Started";

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = (value as string ?? "").Trim();
        var category = PhaseStatusClassifier.Classify(status);

        return category switch
        {
            PhaseStatusCategory.Done => DoneDisplay,
            PhaseStatusCategory.InProgress => InProgressDisplay,
            _ => string.IsNullOrEmpty(status) ? NotStartedDisplay : status
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
