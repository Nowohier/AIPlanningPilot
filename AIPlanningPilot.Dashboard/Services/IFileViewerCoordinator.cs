using CommunityToolkit.Mvvm.ComponentModel;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Coordinates file viewing by dispatching files to the appropriate viewer
/// (markdown, code, docx, drawio) based on their extension.
/// </summary>
public interface IFileViewerCoordinator
{
    /// <summary>
    /// Opens the specified file in the appropriate viewer.
    /// </summary>
    /// <param name="filePath">The absolute path to the file to open.</param>
    /// <returns>The ViewModel of the viewer that opened the file, or <c>null</c> if the file cannot be opened.</returns>
    ObservableObject? OpenFile(string filePath);
}
