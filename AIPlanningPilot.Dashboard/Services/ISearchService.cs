using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Provides full-text search across files in the restructuring directory.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Searches all supported files under the specified root path for the given query.
    /// </summary>
    /// <param name="query">The search text (case-insensitive substring match).</param>
    /// <param name="rootPath">The root directory to search recursively.</param>
    /// <returns>A list of search results with file path, line number, and matched text.</returns>
    List<SearchResult> Search(string query, string rootPath);
}
