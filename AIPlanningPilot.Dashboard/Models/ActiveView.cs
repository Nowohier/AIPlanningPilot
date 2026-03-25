namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Identifies which view is currently displayed in the main content area.
/// </summary>
public enum ActiveView
{
    /// <summary>The project state dashboard.</summary>
    Dashboard,

    /// <summary>The architectural decision records browser.</summary>
    Decisions,

    /// <summary>The handover notes view.</summary>
    Handover,

    /// <summary>The entity migration tracker.</summary>
    Migration,

    /// <summary>A file opened from the tree or search results.</summary>
    File
}
