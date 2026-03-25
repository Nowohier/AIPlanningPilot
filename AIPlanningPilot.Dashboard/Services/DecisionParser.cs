using System.IO;
using System.Text.RegularExpressions;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IDecisionParser"/>.
/// Parses ADR files matching the pattern NNN-*.md in the decisions directory.
/// </summary>
public partial class DecisionParser : IDecisionParser
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecisionParser"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files.</param>
    public DecisionParser(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public List<Decision> ParseAll(string decisionsDirectoryPath)
    {
        var decisions = new List<Decision>();

        // Also parse INDEX.md for the summary table
        var indexPath = Path.Combine(decisionsDirectoryPath, "INDEX.md");
        var indexDecisions = new Dictionary<int, (string Title, string Date, string Affects)>();

        if (fileSystemService.FileExists(indexPath))
        {
            var indexContent = fileSystemService.ReadAllText(indexPath);
            var rows = MarkdownTableParser.ExtractTableUnderHeading(indexContent, "Decisions Index");
            // Fallback: try without "Decisions" prefix -- the heading is just the file title
            if (rows.Count == 0)
            {
                // The INDEX.md has no explicit ## heading before the table, so search for the
                // table by looking for rows directly. Use a broader approach.
                rows = MarkdownTableParser.ExtractFirstTable(indexContent);
            }

            foreach (var row in rows)
            {
                if (row.Length >= 4 && int.TryParse(row[0], out var num))
                {
                    indexDecisions[num] = (row[1], row[2], row[3]);
                }
            }
        }

        // Parse individual ADR files
        var adrFiles = GetAdrFiles(decisionsDirectoryPath);
        foreach (var filePath in adrFiles)
        {
            var decision = ParseAdrFile(filePath);
            if (decision != null)
            {
                // Supplement with INDEX.md data if available
                if (indexDecisions.TryGetValue(decision.Number, out var indexData))
                {
                    if (string.IsNullOrEmpty(decision.Affects))
                    {
                        decision.Affects = indexData.Affects;
                    }
                }
                decisions.Add(decision);
            }
        }

        return decisions.OrderBy(d => d.Number).ToList();
    }

    /// <summary>
    /// Parses a single ADR file into a <see cref="Decision"/> model.
    /// </summary>
    private Decision? ParseAdrFile(string filePath)
    {
        var content = fileSystemService.ReadAllText(filePath);

        // Extract number and title from the first heading: # Decision NNN: Title
        var titleMatch = TitlePattern().Match(content);
        if (!titleMatch.Success)
        {
            return null;
        }

        return new Decision
        {
            Number = int.Parse(titleMatch.Groups[1].Value),
            Title = titleMatch.Groups[2].Value.Trim(),
            Date = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyDate),
            Phase = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyPhase),
            Participants = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyParticipants),
            Context = MarkdownTableParser.ExtractParagraphUnderHeading(content, ParserConstants.SectionContext),
            DecisionText = MarkdownTableParser.ExtractParagraphUnderHeading(content, ParserConstants.SectionDecision),
            FilePath = filePath
        };
    }

    /// <summary>
    /// Gets all ADR files matching the NNN-*.md pattern, sorted by number.
    /// </summary>
    private List<string> GetAdrFiles(string directoryPath)
    {
        if (!fileSystemService.DirectoryExists(directoryPath))
        {
            return [];
        }

        // We need to list files -- use the file system service's tree and filter
        var tree = fileSystemService.GetDirectoryTree(directoryPath);
        return tree
            .Where(n => !n.IsDirectory && AdrFilePattern().IsMatch(n.Name))
            .OrderBy(n => n.Name)
            .Select(n => n.FullPath)
            .ToList();
    }

    [GeneratedRegex(@"#\s*Decision\s+(\d+)\s*:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex TitlePattern();

    [GeneratedRegex(@"^\d{3}-.*\.md$")]
    private static partial Regex AdrFilePattern();
}
