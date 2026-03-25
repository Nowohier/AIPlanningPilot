using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IActionHistoryParser"/>.
/// Parses the completed actions table from archive/completed-actions.md.
/// </summary>
public class ActionHistoryParser : IActionHistoryParser
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionHistoryParser"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files.</param>
    public ActionHistoryParser(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public List<CompletedAction> Parse(string filePath)
    {
        var content = fileSystemService.ReadAllText(filePath);
        var actions = new List<CompletedAction>();

        // The table in completed-actions.md is under "# Completed Actions Archive"
        // but the first table may not be under a ## heading, so try both approaches
        var rows = MarkdownTableParser.ExtractTableUnderHeading(content, "Completed Actions Archive");
        if (rows.Count == 0)
        {
            // Fallback: extract the first table in the file
            rows = MarkdownTableParser.ExtractFirstTable(content);
        }

        foreach (var row in rows)
        {
            if (row.Length >= 5)
            {
                actions.Add(new CompletedAction
                {
                    Number = int.TryParse(row[0], out var n) ? n : 0,
                    Description = row[1],
                    Owner = row[2],
                    CompletedDate = row[3],
                    Notes = row[4]
                });
            }
        }

        return actions;
    }

}
