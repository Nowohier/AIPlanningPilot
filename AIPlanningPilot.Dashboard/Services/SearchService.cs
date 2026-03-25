using System.IO;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="ISearchService"/>.
/// Recursively searches supported file types for case-insensitive text matches.
/// </summary>
public class SearchService : ISearchService
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Maximum number of search results to return to prevent excessive memory usage.
    /// </summary>
    private const int MaxResults = 500;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for file system access.</param>
    public SearchService(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public List<SearchResult> Search(string query, string rootPath)
    {
        if (string.IsNullOrWhiteSpace(query) || !fileSystemService.DirectoryExists(rootPath))
        {
            return [];
        }

        var results = new List<SearchResult>();
        SearchDirectory(rootPath, query, results);
        return results;
    }

    /// <summary>
    /// Recursively searches a directory for files containing the query text.
    /// </summary>
    private void SearchDirectory(string directoryPath, string query, List<SearchResult> results)
    {
        if (results.Count >= MaxResults)
        {
            return;
        }

        try
        {
            foreach (var filePath in fileSystemService.GetFiles(directoryPath))
            {
                var extension = Path.GetExtension(filePath);
                if (!FileSystemConstants.SearchableExtensions.Contains(extension))
                {
                    continue;
                }

                SearchFile(filePath, query, results);

                if (results.Count >= MaxResults)
                {
                    return;
                }
            }

            foreach (var subDir in fileSystemService.GetDirectories(directoryPath))
            {
                var dirName = Path.GetFileName(subDir);
                if (!FileSystemConstants.ExcludedDirectories.Contains(dirName))
                {
                    SearchDirectory(subDir, query, results);
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }
    }

    /// <summary>
    /// Searches a single file for lines containing the query text.
    /// </summary>
    private void SearchFile(string filePath, string query, List<SearchResult> results)
    {
        try
        {
            var lines = fileSystemService.ReadAllLines(filePath);
            var fileName = Path.GetFileName(filePath);

            for (var i = 0; i < lines.Length; i++)
            {
                if (results.Count >= MaxResults)
                {
                    return;
                }

                if (lines[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new SearchResult
                    {
                        FilePath = filePath,
                        FileName = fileName,
                        LineNumber = i + 1,
                        MatchedLine = lines[i].Trim()
                    });
                }
            }
        }
        catch (IOException)
        {
            // Skip files we can't read
        }
    }
}
