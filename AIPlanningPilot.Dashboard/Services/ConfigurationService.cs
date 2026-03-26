using System.IO;
using System.Text.Json;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IConfigurationService"/>.
/// Stores the restructuring root path provided at startup and reads
/// the project name from <c>main/project.json</c>.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="restructuringRootPath">The absolute path to the restructuring directory.</param>
    /// <param name="fileSystemService">Service for file system access.</param>
    public ConfigurationService(string restructuringRootPath, IFileSystemService fileSystemService)
    {
        RestructuringRootPath = restructuringRootPath ?? throw new ArgumentNullException(nameof(restructuringRootPath));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        ProjectName = ResolveProjectName();
    }

    /// <inheritdoc />
    public string RestructuringRootPath { get; }

    /// <inheritdoc />
    public string ProjectName { get; }

    /// <summary>
    /// Reads <c>main/project.json</c> and extracts the <c>projectName</c> value.
    /// This file is written by <c>sync-claude.mjs</c> during <c>/moin</c> or <c>/onboard</c>.
    /// </summary>
    private string ResolveProjectName()
    {
        var configPath = Path.Combine(RestructuringRootPath, "main", "project.json");
        if (!fileSystemService.FileExists(configPath))
        {
            return string.Empty;
        }

        try
        {
            var json = fileSystemService.ReadAllText(configPath);
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("projectName", out var projectNameElement))
            {
                return projectNameElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return string.Empty;
        }
    }
}
