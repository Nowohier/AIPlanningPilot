namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a row from the Phase Progress table in STATE.md.
/// </summary>
public class PhaseProgressItem
{
    /// <summary>Gets or sets the display index (1-based position in the phase list).</summary>
    public int Index { get; set; }

    /// <summary>Gets or sets the phase name (e.g. "Pre-Phase", "Phase 1 (Days 1-4)").</summary>
    public string PhaseName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status text (e.g. "In Progress", "Day 1 done", "Not started").</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the summary description of the phase.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute path to the plan document for this phase (optional, from PlanFile column).</summary>
    public string? PlanFilePath { get; set; }

    /// <summary>Gets or sets the markdown headings (## and ###) extracted from the plan document.</summary>
    public List<PlanHeading> PlanHeadings { get; set; } = [];
}
