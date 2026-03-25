using System.IO;
using System.Windows.Threading;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IFileWatcherService"/>.
/// Wraps <see cref="FileSystemWatcher"/> with debouncing to avoid excessive refresh cycles.
/// </summary>
public class FileWatcherService : IFileWatcherService
{
    private FileSystemWatcher? watcher;
    private DispatcherTimer? debounceTimer;
    private volatile string? lastChangedPath;

    /// <summary>
    /// Debounce interval in milliseconds. Changes within this window are coalesced.
    /// </summary>
    private const int DebounceMs = 500;

    /// <inheritdoc />
    public event Action<string>? FileChanged;

    /// <inheritdoc />
    public bool IsWatching => watcher is { EnableRaisingEvents: true };

    /// <inheritdoc />
    public void Start(string directoryPath)
    {
        Stop();

        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        watcher = new FileSystemWatcher(directoryPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        watcher.Changed += OnFileSystemEvent;
        watcher.Created += OnFileSystemEvent;
        watcher.Deleted += OnFileSystemEvent;
        watcher.Renamed += OnFileSystemRenamed;

        debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DebounceMs)
        };
        debounceTimer.Tick += OnDebounceTimerTick;
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (watcher is not null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnFileSystemEvent;
            watcher.Created -= OnFileSystemEvent;
            watcher.Deleted -= OnFileSystemEvent;
            watcher.Renamed -= OnFileSystemRenamed;
            watcher.Dispose();
            watcher = null;
        }

        if (debounceTimer is not null)
        {
            debounceTimer.Stop();
            debounceTimer.Tick -= OnDebounceTimerTick;
            debounceTimer = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handles file system change, create, and delete events.
    /// </summary>
    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        if (!IsWatchedExtension(e.FullPath))
        {
            return;
        }

        lastChangedPath = e.FullPath;
        RestartDebounceTimer();
    }

    /// <summary>
    /// Handles file rename events.
    /// </summary>
    private void OnFileSystemRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsWatchedExtension(e.FullPath))
        {
            return;
        }

        lastChangedPath = e.FullPath;
        RestartDebounceTimer();
    }

    /// <summary>
    /// Restarts the debounce timer. Must be called on the UI thread via Dispatcher.
    /// </summary>
    private void RestartDebounceTimer()
    {
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            debounceTimer?.Stop();
            debounceTimer?.Start();
        });
    }

    /// <summary>
    /// Fires the debounced <see cref="FileChanged"/> event when the timer elapses.
    /// </summary>
    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        debounceTimer?.Stop();
        if (lastChangedPath is not null)
        {
            FileChanged?.Invoke(lastChangedPath);
        }
    }

    /// <summary>
    /// Checks if a file path has a watched extension.
    /// </summary>
    private static bool IsWatchedExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return FileSystemConstants.WatchedExtensions.Contains(extension);
    }
}
