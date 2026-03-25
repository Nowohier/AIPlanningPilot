using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Parses ADR files from the decisions/ directory into <see cref="Decision"/> models.
/// </summary>
public interface IDecisionParser
{
    /// <summary>
    /// Parses all ADR files in the specified decisions directory.
    /// </summary>
    /// <param name="decisionsDirectoryPath">The absolute path to the decisions/ directory.</param>
    /// <returns>A list of parsed decisions, sorted by number.</returns>
    List<Decision> ParseAll(string decisionsDirectoryPath);
}
