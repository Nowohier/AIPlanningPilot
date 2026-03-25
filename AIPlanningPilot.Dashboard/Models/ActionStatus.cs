namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents the status of an action item in the Next Actions table.
/// </summary>
public enum ActionStatus
{
    /// <summary>The action is pending and not yet started.</summary>
    Pending,

    /// <summary>The action is the next item to work on.</summary>
    Next,

    /// <summary>The action has been completed.</summary>
    Done
}
