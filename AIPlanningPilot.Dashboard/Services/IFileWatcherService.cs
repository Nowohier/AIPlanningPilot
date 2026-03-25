namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Watches a directory for file changes and raises events when files are modified.
/// Debounces rapid changes to avoid excessive refresh cycles.
/// </summary>
public interface IFileWatcherService : IDisposable
{
    /// <summary>
    /// Occurs when a file in the watched directory is created, modified, or deleted.
    /// The string parameter is the full path of the changed file.
    /// </summary>
    event Action<string>? FileChanged;

    /// <summary>
    /// Gets a value indicating whether the watcher is currently active.
    /// </summary>
    bool IsWatching { get; }

    /// <summary>
    /// Starts watching the specified directory for file changes.
    /// </summary>
    /// <param name="directoryPath">The directory to watch recursively.</param>
    void Start(string directoryPath);

    /// <summary>
    /// Stops watching for file changes.
    /// </summary>
    void Stop();
}
