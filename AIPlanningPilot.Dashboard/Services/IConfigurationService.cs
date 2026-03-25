namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Provides access to application configuration, primarily the restructuring root directory path.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the absolute path to the restructuring directory root.
    /// </summary>
    string RestructuringRootPath { get; }

    /// <summary>
    /// Gets the name of the project being orchestrated, derived from the
    /// <c>${PROJECT_REPO}</c> variable in <c>main/CONFIG.md</c>.
    /// Returns an empty string if the value cannot be determined.
    /// </summary>
    string ProjectName { get; }
}
