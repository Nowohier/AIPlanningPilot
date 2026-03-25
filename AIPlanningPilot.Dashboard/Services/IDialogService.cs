namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Abstraction for showing application dialogs, decoupling ViewModels from View types.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows the settings dialog as a modal window.
    /// </summary>
    void ShowSettingsDialog();
}
