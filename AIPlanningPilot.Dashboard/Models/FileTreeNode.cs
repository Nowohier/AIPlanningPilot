using System.IO;

namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a node in the file system tree, which can be either a directory or a file.
/// </summary>
public class FileTreeNode
{
    /// <summary>
    /// Gets or sets the display name of the file or directory.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the full absolute path to the file or directory.
    /// </summary>
    public required string FullPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this node represents a directory.
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Gets or sets the child nodes. Only populated for directory nodes.
    /// </summary>
    public List<FileTreeNode> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the last modification time of the file or directory.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets the file extension in lowercase, or an empty string for directories.
    /// </summary>
    public string Extension => IsDirectory ? string.Empty : Path.GetExtension(Name).ToLowerInvariant();
}
