using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Parses the archive/completed-actions.md file into a list of <see cref="CompletedAction"/> models.
/// </summary>
public interface IActionHistoryParser
{
    /// <summary>
    /// Parses the completed actions archive file.
    /// </summary>
    /// <param name="filePath">The absolute path to archive/completed-actions.md.</param>
    /// <returns>A list of completed actions.</returns>
    List<CompletedAction> Parse(string filePath);
}
