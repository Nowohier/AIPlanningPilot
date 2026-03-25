namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Manages persistent application settings stored in a JSON file.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets or sets the selected markdown theme name.
    /// </summary>
    string SelectedThemeName { get; set; }

    /// <summary>
    /// Loads settings from the settings file. Uses defaults if the file does not exist.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves current settings to the settings file.
    /// </summary>
    void Save();
}
