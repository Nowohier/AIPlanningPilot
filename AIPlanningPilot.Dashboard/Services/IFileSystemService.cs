using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Abstraction over file system operations for testability.
/// Provides methods for reading files, listing directories, and querying file metadata.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Reads the entire contents of a file as a string.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    /// <returns>The file contents.</returns>
    string ReadAllText(string filePath);

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <returns><c>true</c> if the directory exists; otherwise, <c>false</c>.</returns>
    bool DirectoryExists(string directoryPath);

    /// <summary>
    /// Builds a tree of <see cref="FileTreeNode"/> objects representing the directory structure
    /// starting from the specified root path.
    /// </summary>
    /// <param name="rootPath">The root directory to scan.</param>
    /// <param name="applyWhitelist">
    /// When <c>true</c>, applies a root-level whitelist to show only allowed directories and files.
    /// Used by the TreeView. Parsers should pass <c>false</c> to see all files.
    /// </param>
    /// <returns>A list of top-level nodes representing the directory contents.</returns>
    List<FileTreeNode> GetDirectoryTree(string rootPath, bool applyWhitelist = false);

    /// <summary>
    /// Gets the last write time of the specified file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The last write time in UTC.</returns>
    DateTime GetLastWriteTimeUtc(string filePath);

    /// <summary>
    /// Reads all lines from a file.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    /// <returns>An array of all lines in the file.</returns>
    string[] ReadAllLines(string filePath);

    /// <summary>
    /// Writes the specified text to a file, creating or overwriting it.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    /// <param name="contents">The text to write.</param>
    void WriteAllText(string filePath, string contents);

    /// <summary>
    /// Creates all directories in the specified path if they do not exist.
    /// </summary>
    /// <param name="directoryPath">The directory path to create.</param>
    void CreateDirectory(string directoryPath);

    /// <summary>
    /// Gets the files in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The directory to get files from.</param>
    /// <returns>An array of file paths.</returns>
    string[] GetFiles(string directoryPath);

    /// <summary>
    /// Gets the subdirectories of the specified directory.
    /// </summary>
    /// <param name="directoryPath">The directory to get subdirectories from.</param>
    /// <returns>An array of directory paths.</returns>
    string[] GetDirectories(string directoryPath);
}
