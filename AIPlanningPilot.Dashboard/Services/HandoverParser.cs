using System.IO;
using System.Text.RegularExpressions;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IHandoverParser"/>.
/// Parses handover-{name}.md files for per-developer session notes.
/// </summary>
public partial class HandoverParser : IHandoverParser
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandoverParser"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files.</param>
    public HandoverParser(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public List<HandoverNotes> ParseAll(string handoversDirectoryPath)
    {
        var results = new List<HandoverNotes>();

        if (!fileSystemService.DirectoryExists(handoversDirectoryPath))
        {
            return results;
        }

        var tree = fileSystemService.GetDirectoryTree(handoversDirectoryPath);
        var handoverFiles = tree
            .Where(n => !n.IsDirectory && HandoverFilePattern().IsMatch(n.Name))
            .OrderBy(n => n.Name);

        foreach (var file in handoverFiles)
        {
            var notes = ParseFile(file.FullPath, file.Name);
            if (notes != null)
            {
                results.Add(notes);
            }
        }

        return results;
    }

    /// <summary>
    /// Parses a single handover file.
    /// </summary>
    private HandoverNotes? ParseFile(string filePath, string fileName)
    {
        var content = fileSystemService.ReadAllText(filePath);

        // Extract developer name from filename: handover-chris.md -> chris
        var nameMatch = HandoverFilePattern().Match(fileName);
        if (!nameMatch.Success)
        {
            return null;
        }

        return new HandoverNotes
        {
            DeveloperName = nameMatch.Groups[1].Value,
            LastUpdated = ExtractLastUpdated(content),
            ForNextSession = MarkdownTableParser.ExtractItemsUnderHeading(content, ParserConstants.SectionForNextSession),
            DecisionsAndFindings = MarkdownTableParser.ExtractBulletListUnderHeading(content, ParserConstants.SectionDecisionsAndFindings),
            OpenThreads = MarkdownTableParser.ExtractBulletListUnderHeading(content, ParserConstants.SectionOpenThreads),
            FromLastSession = MarkdownTableParser.ExtractItemsUnderHeading(content, ParserConstants.SectionFromLastSession)
        };
    }

    /// <summary>
    /// Extracts the "Last updated" date from a blockquote line.
    /// </summary>
    private static string ExtractLastUpdated(string content)
    {
        var match = LastUpdatedPattern().Match(content);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    [GeneratedRegex(@"^handover-(\w+)\.md$", RegexOptions.IgnoreCase)]
    private static partial Regex HandoverFilePattern();

    [GeneratedRegex(@">\s*Last updated:\s*(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex LastUpdatedPattern();
}
