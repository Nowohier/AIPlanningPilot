namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="INavigationService"/>.
/// Uses events to decouple file selection from view display.
/// </summary>
public class NavigationService : INavigationService
{
    /// <inheritdoc />
    public event EventHandler<string>? FileSelected;

    /// <inheritdoc />
    public void NavigateToFile(string filePath)
    {
        FileSelected?.Invoke(this, filePath);
    }
}
