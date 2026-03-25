namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a parsed Architectural Decision Record (ADR) from the decisions/ directory.
/// </summary>
public class Decision
{
    /// <summary>Gets or sets the ADR number (e.g. 0, 1, 2...).</summary>
    public int Number { get; set; }

    /// <summary>Gets or sets the decision title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the decision date.</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Gets or sets which phase the decision was made in.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets who participated in the decision.</summary>
    public string Participants { get; set; } = string.Empty;

    /// <summary>Gets or sets the context section content.</summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>Gets or sets the decision text.</summary>
    public string DecisionText { get; set; } = string.Empty;

    /// <summary>Gets or sets what areas the decision affects.</summary>
    public string Affects { get; set; } = string.Empty;

    /// <summary>Gets or sets the source file path.</summary>
    public string FilePath { get; set; } = string.Empty;
}
