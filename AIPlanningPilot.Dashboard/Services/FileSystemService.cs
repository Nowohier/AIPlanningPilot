using System.IO;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IFileSystemService"/> using <see cref="System.IO"/>.
/// Applies a root-level whitelist to filter which directories and files are shown in the tree.
/// </summary>
public class FileSystemService : IFileSystemService
{
    /// <summary>
    /// Root-level directories allowed in the tree view (all contents are shown within these).
    /// </summary>
    private static readonly HashSet<string> AllowedRootDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "analysis", "archive", "claude-backup", "decisions", "documents",
        "handovers", "main", "plan", "scripts"
    };

    /// <summary>
    /// Root-level files allowed in the tree view.
    /// </summary>
    private static readonly HashSet<string> AllowedRootFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Readme.md"
    };

    /// <inheritdoc />
    public string ReadAllText(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    /// <inheritdoc />
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <inheritdoc />
    public bool DirectoryExists(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    /// <inheritdoc />
    public List<FileTreeNode> GetDirectoryTree(string rootPath, bool applyWhitelist = false)
    {
        if (!Directory.Exists(rootPath))
        {
            return [];
        }

        return applyWhitelist ? BuildRootTree(rootPath) : BuildSubTree(rootPath);
    }

    /// <inheritdoc />
    public DateTime GetLastWriteTimeUtc(string filePath)
    {
        return File.GetLastWriteTimeUtc(filePath);
    }

    /// <inheritdoc />
    public string[] ReadAllLines(string filePath)
    {
        return File.ReadAllLines(filePath);
    }

    /// <inheritdoc />
    public void WriteAllText(string filePath, string contents)
    {
        File.WriteAllText(filePath, contents);
    }

    /// <inheritdoc />
    public void CreateDirectory(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }

    /// <inheritdoc />
    public string[] GetFiles(string directoryPath)
    {
        return Directory.GetFiles(directoryPath);
    }

    /// <inheritdoc />
    public string[] GetDirectories(string directoryPath)
    {
        return Directory.GetDirectories(directoryPath);
    }

    /// <summary>
    /// Builds the root-level tree with whitelist filtering.
    /// Only directories in <see cref="AllowedRootDirectories"/> and files in
    /// <see cref="AllowedRootFiles"/> are included at the root level.
    /// </summary>
    private static List<FileTreeNode> BuildRootTree(string rootPath)
    {
        var nodes = new List<FileTreeNode>();

        // Add whitelisted directories, sorted alphabetically
        var directories = Directory.GetDirectories(rootPath)
            .Select(d => new DirectoryInfo(d))
            .Where(d => AllowedRootDirectories.Contains(d.Name))
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var dir in directories)
        {
            nodes.Add(new FileTreeNode
            {
                Name = dir.Name,
                FullPath = dir.FullName,
                IsDirectory = true,
                LastModified = dir.LastWriteTimeUtc,
                Children = BuildSubTree(dir.FullName)
            });
        }

        // Add whitelisted root files, sorted alphabetically
        var files = Directory.GetFiles(rootPath)
            .Select(f => new FileInfo(f))
            .Where(f => AllowedRootFiles.Contains(f.Name))
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            nodes.Add(new FileTreeNode
            {
                Name = file.Name,
                FullPath = file.FullName,
                IsDirectory = false,
                LastModified = file.LastWriteTimeUtc
            });
        }

        return nodes;
    }

    /// <summary>
    /// Recursively builds a tree for subdirectories. Shows all contents
    /// except excluded directories like .git, bin, obj, etc.
    /// </summary>
    private static List<FileTreeNode> BuildSubTree(string directoryPath)
    {
        var nodes = new List<FileTreeNode>();

        var directories = Directory.GetDirectories(directoryPath)
            .Select(d => new DirectoryInfo(d))
            .Where(d => !FileSystemConstants.ExcludedDirectories.Contains(d.Name))
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var dir in directories)
        {
            nodes.Add(new FileTreeNode
            {
                Name = dir.Name,
                FullPath = dir.FullName,
                IsDirectory = true,
                LastModified = dir.LastWriteTimeUtc,
                Children = BuildSubTree(dir.FullName)
            });
        }

        var files = Directory.GetFiles(directoryPath)
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            nodes.Add(new FileTreeNode
            {
                Name = file.Name,
                FullPath = file.FullName,
                IsDirectory = false,
                LastModified = file.LastWriteTimeUtc
            });
        }

        return nodes;
    }
}
