using System.IO;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Resolves the restructuring root directory from command-line arguments
/// or by walking up the directory tree to find a known marker file.
/// </summary>
internal static class RootPathResolver
{
    /// <summary>
    /// Resolves the restructuring root path from command-line arguments
    /// or falls back to walking up the directory tree from the executable location.
    /// </summary>
    /// <param name="args">Command-line arguments. The first argument, if provided, is used as the root path.</param>
    /// <param name="baseDirectory">The base directory to start searching from (typically the executable directory).</param>
    /// <returns>The absolute path to the restructuring directory.</returns>
    public static string Resolve(string[] args, string baseDirectory)
    {
        if (args.Length > 0 && Directory.Exists(args[0]))
        {
            return Path.GetFullPath(args[0]);
        }

        // Walk up from the base directory looking for a sibling containing main/STATE.md
        var current = baseDirectory;
        for (var i = 0; i < 10; i++)
        {
            var parent = Path.GetDirectoryName(current);
            if (parent is null || parent == current)
            {
                break;
            }

            current = parent;

            if (Directory.Exists(current))
            {
                foreach (var dir in Directory.GetDirectories(current))
                {
                    if (File.Exists(Path.Combine(dir, "main", "STATE.md")))
                    {
                        return dir;
                    }
                }
            }
        }

        // Fallback: current directory
        return Directory.GetCurrentDirectory();
    }
}
