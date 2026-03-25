namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a row from the Next Actions table in STATE.md.
/// </summary>
public class ActionItem : ActionBase
{
    /// <summary>Gets or sets the current status of the action.</summary>
    public ActionStatus Status { get; set; }
}
