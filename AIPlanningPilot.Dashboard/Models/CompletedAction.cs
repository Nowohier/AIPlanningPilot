namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a row from the completed actions archive table.
/// </summary>
public class CompletedAction : ActionBase
{
    /// <summary>Gets or sets the completion date string.</summary>
    public string CompletedDate { get; set; } = string.Empty;
}
