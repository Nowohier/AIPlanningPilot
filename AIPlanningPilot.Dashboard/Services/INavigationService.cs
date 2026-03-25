namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Coordinates navigation between views in the application.
/// Raises events when the selected file or active view changes.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Occurs when a file is selected for viewing.
    /// </summary>
    event EventHandler<string>? FileSelected;

    /// <summary>
    /// Navigates to the specified file, notifying all subscribers.
    /// </summary>
    /// <param name="filePath">The absolute path to the file to display.</param>
    void NavigateToFile(string filePath);
}
