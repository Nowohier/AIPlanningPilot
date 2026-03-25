namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a single search match found within a file.
/// </summary>
public class SearchResult
{
    /// <summary>Gets or sets the full path to the file containing the match.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the file name for display.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the 1-based line number of the match.</summary>
    public int LineNumber { get; set; }

    /// <summary>Gets or sets the matched line text (trimmed).</summary>
    public string MatchedLine { get; set; } = string.Empty;
}
