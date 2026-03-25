namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents an open decision from the Open Decisions table in STATE.md.
/// </summary>
public class OpenDecision
{
    /// <summary>Gets or sets the decision description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets when the decision is expected (e.g. "Phase 1").</summary>
    public string When { get; set; } = string.Empty;

    /// <summary>Gets or sets the impact level.</summary>
    public ImpactLevel Impact { get; set; } = ImpactLevel.Medium;
}
