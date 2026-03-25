namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents the parsed content of STATE.md, the morning briefing document.
/// Contains the current project phase, actions, progress, and open decisions.
/// </summary>
public class ProjectState
{
    /// <summary>Gets or sets the current phase name (e.g. "Phase 1 -- Deep Analysis &amp; Architecture").</summary>
    public string CurrentPhase { get; set; } = string.Empty;

    /// <summary>Gets or sets the current day number.</summary>
    public int Day { get; set; }

    /// <summary>Gets or sets the last updated date string.</summary>
    public string LastUpdated { get; set; } = string.Empty;

    /// <summary>Gets or sets the active branch name.</summary>
    public string Branch { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of next actions.</summary>
    public List<ActionItem> NextActions { get; set; } = [];

    /// <summary>Gets or sets the phase progress items.</summary>
    public List<PhaseProgressItem> PhaseProgress { get; set; } = [];

    /// <summary>Gets or sets the list of open decisions.</summary>
    public List<OpenDecision> OpenDecisions { get; set; } = [];

    /// <summary>Gets or sets the decisions summary text.</summary>
    public string DecisionsSummary { get; set; } = string.Empty;

    /// <summary>Gets or sets the team members description.</summary>
    public List<string> TeamMembers { get; set; } = [];
}
