namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents parsed content from a handover-{name}.md file.
/// Contains per-developer session notes and action items for the next session.
/// </summary>
public class HandoverNotes
{
    /// <summary>Gets or sets the developer name.</summary>
    public string DeveloperName { get; set; } = string.Empty;

    /// <summary>Gets or sets the last updated date string.</summary>
    public string LastUpdated { get; set; } = string.Empty;

    /// <summary>Gets or sets the items listed under "For Next Session".</summary>
    public List<string> ForNextSession { get; set; } = [];

    /// <summary>Gets or sets the items listed under "Decisions &amp; Findings".</summary>
    public List<string> DecisionsAndFindings { get; set; } = [];

    /// <summary>Gets or sets the items listed under "Open Threads".</summary>
    public List<string> OpenThreads { get; set; } = [];

    /// <summary>Gets or sets the items from the "From Last Session" section.</summary>
    public List<string> FromLastSession { get; set; } = [];

    /// <summary>Gets or sets the dated session log entries from the "Session Log" section.</summary>
    public List<SessionLogEntry> SessionLog { get; set; } = [];
}
