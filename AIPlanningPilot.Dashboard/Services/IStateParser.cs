using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Parses the STATE.md file into a <see cref="ProjectState"/> model.
/// </summary>
public interface IStateParser
{
    /// <summary>
    /// Parses the STATE.md file at the specified path.
    /// </summary>
    /// <param name="filePath">The absolute path to STATE.md.</param>
    /// <returns>A <see cref="ProjectState"/> containing the parsed data.</returns>
    ProjectState Parse(string filePath);
}
