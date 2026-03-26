namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a single dated entry in the Session Log section of a handover file.
/// Each entry corresponds to one work session (typically one day).
/// </summary>
public class SessionLogEntry
{
    /// <summary>Gets or sets the date string for this session entry (YYYY-MM-DD format).</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Gets or sets the bullet-point items summarizing the session.</summary>
    public List<string> Items { get; set; } = [];
}
