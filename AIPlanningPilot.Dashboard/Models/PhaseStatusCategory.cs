namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// The possible categories for a phase status.
/// </summary>
public enum PhaseStatusCategory
{
    /// <summary>The phase is complete.</summary>
    Done,

    /// <summary>The phase is in progress.</summary>
    InProgress,

    /// <summary>The phase has not started.</summary>
    NotStarted
}
