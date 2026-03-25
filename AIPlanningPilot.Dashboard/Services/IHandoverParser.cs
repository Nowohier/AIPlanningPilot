using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Parses handover-{name}.md files into <see cref="HandoverNotes"/> models.
/// </summary>
public interface IHandoverParser
{
    /// <summary>
    /// Parses all handover files in the specified directory.
    /// </summary>
    /// <param name="handoversDirectoryPath">The absolute path to the handovers/ directory.</param>
    /// <returns>A list of parsed handover notes, one per developer.</returns>
    List<HandoverNotes> ParseAll(string handoversDirectoryPath);
}
