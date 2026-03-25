namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Shared constants for file system operations used across multiple services.
/// </summary>
internal static class FileSystemConstants
{
    /// <summary>
    /// Directories to exclude from directory traversal and search operations.
    /// </summary>
    public static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "node_modules", "bin", "obj", ".vs"
    };

    /// <summary>
    /// Base set of file extensions used by file watching.
    /// </summary>
    public static readonly HashSet<string> WatchedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".sh", ".bat", ".cmd", ".js", ".mjs", ".json"
    };

    /// <summary>
    /// Extended set of file extensions used by search (superset of watched extensions).
    /// </summary>
    public static readonly HashSet<string> SearchableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".sh", ".bat", ".cmd", ".js", ".mjs", ".json", ".cs", ".xml", ".xaml"
    };
}
