using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Parses the MIGRATION.md file into a list of <see cref="MigrationEntity"/> records.
/// </summary>
public interface IMigrationParser
{
    /// <summary>
    /// Parses the MIGRATION.md file at the specified path.
    /// </summary>
    /// <param name="filePath">The absolute path to MIGRATION.md.</param>
    /// <returns>A list of parsed migration entity records.</returns>
    List<MigrationEntity> Parse(string filePath);
}
