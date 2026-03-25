using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Classifies phase status strings into discrete categories for consistent
/// color/brush selection across multiple converters.
/// </summary>
internal static class PhaseStatusClassifier
{
    private const string DoneStatus = "done";
    private const string CompleteStatus = "complete";
    private const string CompletedStatus = "completed";
    private const string NotStartedStatus = "not started";

    /// <summary>
    /// Classifies the given status string into a <see cref="PhaseStatusCategory"/>.
    /// </summary>
    /// <param name="status">The status text from the markdown table.</param>
    /// <returns>The classified phase status category.</returns>
    public static PhaseStatusCategory Classify(string? status)
    {
        var normalized = (status ?? "").Trim().ToLowerInvariant();

        if (normalized is DoneStatus or CompleteStatus or CompletedStatus)
        {
            return PhaseStatusCategory.Done;
        }

        if (normalized != NotStartedStatus && normalized.Length > 0)
        {
            return PhaseStatusCategory.InProgress;
        }

        return PhaseStatusCategory.NotStarted;
    }
}
