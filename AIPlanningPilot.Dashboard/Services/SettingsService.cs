using System.IO;
using System.Text.Json;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Manages persistent application settings stored as JSON in the user's local application data folder.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IFileSystemService fileSystemService;

    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIPlanningPilot.Dashboard");

    private static readonly string SettingsFilePath =
        Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private AppSettings settings = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for file system access.</param>
    public SettingsService(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public string SelectedThemeName
    {
        get => settings.SelectedThemeName;
        set => settings.SelectedThemeName = value;
    }

    /// <inheritdoc />
    public void Load()
    {
        if (!fileSystemService.FileExists(SettingsFilePath))
        {
            settings = new AppSettings();
            return;
        }

        try
        {
            var json = fileSystemService.ReadAllText(SettingsFilePath);
            settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or NotSupportedException)
        {
            settings = new AppSettings();
        }
    }

    /// <inheritdoc />
    public void Save()
    {
        fileSystemService.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        fileSystemService.WriteAllText(SettingsFilePath, json);
    }

    /// <summary>
    /// Internal model for JSON serialization of application settings.
    /// </summary>
    private sealed class AppSettings
    {
        /// <summary>Gets or sets the selected markdown theme name.</summary>
        public string SelectedThemeName { get; set; } = ParserConstants.DefaultThemeName;
    }
}
