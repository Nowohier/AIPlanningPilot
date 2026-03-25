namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Base class for action items, containing shared properties between active and completed actions.
/// </summary>
public abstract class ActionBase
{
    /// <summary>Gets or sets the action number.</summary>
    public int Number { get; set; }

    /// <summary>Gets or sets the action description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the owner of the action.</summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>Gets or sets additional notes.</summary>
    public string Notes { get; set; } = string.Empty;
}
