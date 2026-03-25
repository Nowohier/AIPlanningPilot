namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a heading extracted from a plan markdown document.
/// </summary>
public class PlanHeading
{
    /// <summary>Gets or sets the heading level (2 for ##, 3 for ###).</summary>
    public int Level { get; set; }

    /// <summary>Gets or sets the heading text (without the # prefix).</summary>
    public string Text { get; set; } = string.Empty;
}
