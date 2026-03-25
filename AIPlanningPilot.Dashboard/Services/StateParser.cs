using System.IO;
using System.Text.RegularExpressions;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IStateParser"/>.
/// Parses STATE.md tables and blockquote metadata into a <see cref="ProjectState"/>.
/// </summary>
public class StateParser : IStateParser
{
    private readonly IFileSystemService fileSystemService;
    private readonly IConfigurationService configurationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateParser"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files.</param>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    public StateParser(IFileSystemService fileSystemService, IConfigurationService configurationService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <inheritdoc />
    public ProjectState Parse(string filePath)
    {
        var content = fileSystemService.ReadAllText(filePath);
        var state = ParseMetadata(content);
        ParseNextActions(content, state);
        ParsePhaseProgress(content, state);
        ParseOpenDecisions(content, state);
        ParseTeamMembers(content, state);
        return state;
    }

    /// <summary>
    /// Parses the blockquote metadata values (phase, last updated, branch, day) from STATE.md content.
    /// </summary>
    /// <param name="content">The full STATE.md content.</param>
    /// <returns>A new <see cref="ProjectState"/> populated with metadata values.</returns>
    private static ProjectState ParseMetadata(string content)
    {
        var state = new ProjectState
        {
            CurrentPhase = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyPhase),
            LastUpdated = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyLastUpdated),
            Branch = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyBranch),
        };

        var dayStr = MarkdownTableParser.ExtractBlockquoteValue(content, ParserConstants.KeyDay);
        if (int.TryParse(Regex.Match(dayStr, @"\d+").Value, out var day))
        {
            state.Day = day;
        }

        return state;
    }

    /// <summary>
    /// Parses the Next Actions table and populates the state's action items.
    /// </summary>
    /// <param name="content">The full STATE.md content.</param>
    /// <param name="state">The project state to populate.</param>
    private static void ParseNextActions(string content, ProjectState state)
    {
        var actionRows = MarkdownTableParser.ExtractTableUnderHeading(content, ParserConstants.SectionNextActions);
        foreach (var row in actionRows)
        {
            if (row.Length >= 4)
            {
                state.NextActions.Add(new ActionItem
                {
                    Number = int.TryParse(row[0], out var n) ? n : 0,
                    Description = row[1],
                    Owner = row[2],
                    Status = ParseActionStatus(row[3]),
                    Notes = row.Length > 4 ? row[4] : string.Empty
                });
            }
        }
    }

    /// <summary>
    /// Parses the Phase Progress table and populates the state's phase progress items.
    /// </summary>
    /// <param name="content">The full STATE.md content.</param>
    /// <param name="state">The project state to populate.</param>
    private void ParsePhaseProgress(string content, ProjectState state)
    {
        var progressRows = MarkdownTableParser.ExtractTableUnderHeading(content, ParserConstants.SectionPhaseProgress);
        var phaseIndex = 0;
        foreach (var row in progressRows)
        {
            if (row.Length >= 3)
            {
                phaseIndex++;
                var item = new PhaseProgressItem
                {
                    Index = phaseIndex,
                    PhaseName = row[0],
                    Status = row[1],
                    Summary = row[2]
                };

                if (row.Length >= 4 && !string.IsNullOrWhiteSpace(row[3]))
                {
                    var rootPath = configurationService.RestructuringRootPath;
                    var absolutePath = Path.Combine(rootPath, row[3].Trim());
                    if (fileSystemService.FileExists(absolutePath))
                    {
                        item.PlanFilePath = absolutePath;
                        var planContent = fileSystemService.ReadAllText(absolutePath);
                        item.PlanHeadings = ExtractMarkdownHeadings(planContent);
                    }
                }

                state.PhaseProgress.Add(item);
            }
        }
    }

    /// <summary>
    /// Parses the Open Decisions table and populates the state's open decisions.
    /// </summary>
    /// <param name="content">The full STATE.md content.</param>
    /// <param name="state">The project state to populate.</param>
    private static void ParseOpenDecisions(string content, ProjectState state)
    {
        var decisionRows = MarkdownTableParser.ExtractTableUnderHeading(content, ParserConstants.SectionOpenDecisions);
        foreach (var row in decisionRows)
        {
            if (row.Length >= 3)
            {
                state.OpenDecisions.Add(new OpenDecision
                {
                    Description = row[0],
                    When = row[1],
                    Impact = ParseImpactLevel(row[2])
                });
            }
        }
    }

    /// <summary>
    /// Parses the Team/Branches section and populates the state's team member list.
    /// </summary>
    /// <param name="content">The full STATE.md content.</param>
    /// <param name="state">The project state to populate.</param>
    private static void ParseTeamMembers(string content, ProjectState state)
    {
        var teamLines = MarkdownTableParser.ExtractParagraphUnderHeading(content, ParserConstants.SectionTeamBranches);
        if (!string.IsNullOrEmpty(teamLines))
        {
            // Match bold names followed by a hyphen, en-dash, or em-dash
            var matches = Regex.Matches(teamLines, @"\*\*(\w[\w\s]*?)\*\*\s*[-\u2013\u2014]");
            foreach (Match match in matches)
            {
                state.TeamMembers.Add(match.Groups[1].Value.Trim());
            }
        }
    }

    /// <summary>
    /// Extracts ## and ### headings from a markdown file.
    /// </summary>
    /// <param name="content">The markdown content to extract headings from.</param>
    /// <returns>List of heading texts (without the # prefix).</returns>
    private static List<PlanHeading> ExtractMarkdownHeadings(string content)
    {
        var headings = new List<PlanHeading>();
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("### "))
            {
                headings.Add(new PlanHeading { Level = 3, Text = trimmed[4..].Trim() });
            }
            else if (trimmed.StartsWith("## "))
            {
                headings.Add(new PlanHeading { Level = 2, Text = trimmed[3..].Trim() });
            }
        }
        return headings;
    }

    /// <summary>
    /// Parses the action status from the text in the Status column.
    /// Recognizes bold "Done", "Next", and plain "Pending".
    /// </summary>
    private static ActionStatus ParseActionStatus(string statusText)
    {
        var lower = statusText.ToLowerInvariant();
        if (lower.Contains("done"))
        {
            return ActionStatus.Done;
        }
        if (lower.Contains("next"))
        {
            return ActionStatus.Next;
        }
        return ActionStatus.Pending;
    }

    /// <summary>
    /// Parses the impact level from the text in the Impact column.
    /// </summary>
    private static ImpactLevel ParseImpactLevel(string impactText)
    {
        return impactText.Trim().ToLowerInvariant() switch
        {
            "very high" => ImpactLevel.VeryHigh,
            "high" => ImpactLevel.High,
            "medium" => ImpactLevel.Medium,
            "low" => ImpactLevel.Low,
            _ => ImpactLevel.Medium
        };
    }
}
