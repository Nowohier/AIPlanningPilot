using System.Text.RegularExpressions;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Utility for parsing markdown tables into structured row data.
/// Operates on raw text lines rather than a Markdown AST for reliability with
/// the specific table formats used in the restructuring documents.
/// </summary>
internal static partial class MarkdownTableParser
{
    /// <summary>
    /// Extracts all rows from a markdown table found under the specified heading.
    /// </summary>
    /// <param name="markdownText">The full markdown document text.</param>
    /// <param name="headingText">The heading text to search for (e.g. "Next Actions"). Matched case-insensitively.</param>
    /// <returns>
    /// A list of string arrays, one per data row. Each array contains the cell values
    /// with leading/trailing whitespace and markdown formatting stripped.
    /// Returns an empty list if the heading or table is not found.
    /// </returns>
    public static List<string[]> ExtractTableUnderHeading(string markdownText, string headingText)
    {
        var lines = markdownText.Split('\n');
        var rows = new List<string[]>();
        var foundHeading = false;
        var foundTable = false;
        var skippedSeparator = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');

            if (!foundHeading)
            {
                // Look for the heading (## or ###)
                if (IsHeading(line, headingText))
                {
                    foundHeading = true;
                }
                continue;
            }

            // After heading found, look for table rows (lines starting with |)
            if (!foundTable)
            {
                if (line.TrimStart().StartsWith('|'))
                {
                    foundTable = true;
                    // First | line is the header row -- skip it
                    // Next line should be the separator (|---|---|...)
                    continue;
                }

                // If we hit another heading before finding a table, stop
                if (line.TrimStart().StartsWith('#'))
                {
                    break;
                }
                continue;
            }

            // We're inside the table
            if (!skippedSeparator)
            {
                // Skip the separator row (|---|---|...)
                if (SeparatorPattern().IsMatch(line))
                {
                    skippedSeparator = true;
                    continue;
                }
            }

            if (!line.TrimStart().StartsWith('|'))
            {
                // End of table
                break;
            }

            // Parse the data row
            var cells = ParseTableRow(line);
            if (cells.Length > 0)
            {
                rows.Add(cells);
            }
        }

        return rows;
    }

    /// <summary>
    /// Extracts the first markdown table found in the content, regardless of heading.
    /// Skips the header row and separator, returning only data rows.
    /// </summary>
    /// <param name="content">The full markdown text.</param>
    /// <returns>
    /// A list of string arrays, one per data row, with cell values stripped of formatting.
    /// Returns an empty list if no table is found.
    /// </returns>
    public static List<string[]> ExtractFirstTable(string content)
    {
        var lines = content.Split('\n');
        var rows = new List<string[]>();
        var inTable = false;
        var skippedSeparator = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            if (!inTable && line.TrimStart().StartsWith('|'))
            {
                inTable = true;
                continue; // Skip header
            }

            if (!inTable)
            {
                continue;
            }

            if (!line.TrimStart().StartsWith('|'))
            {
                break; // End of table
            }

            if (!skippedSeparator && SeparatorPattern().IsMatch(line))
            {
                skippedSeparator = true;
                continue;
            }

            var cells = ParseTableRow(line);
            if (cells.Length > 0)
            {
                rows.Add(cells);
            }
        }

        return rows;
    }

    /// <summary>
    /// Extracts text from a blockquote section that matches a key pattern like "**Phase**:".
    /// </summary>
    /// <param name="markdownText">The full markdown text.</param>
    /// <param name="key">The bold key to search for (e.g. "Phase", "Day").</param>
    /// <returns>The value text after the colon, or an empty string if not found.</returns>
    public static string ExtractBlockquoteValue(string markdownText, string key)
    {
        var pattern = $@"\*\*{Regex.Escape(key)}\*\*:\s*(.+)";
        var match = Regex.Match(markdownText, pattern);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    /// <summary>
    /// Extracts bullet list items from under a specified heading.
    /// </summary>
    /// <param name="markdownText">The full markdown text.</param>
    /// <param name="headingText">The heading to find.</param>
    /// <returns>List of bullet item texts with the leading "- " stripped.</returns>
    public static List<string> ExtractBulletListUnderHeading(string markdownText, string headingText)
    {
        var lines = markdownText.Split('\n');
        var items = new List<string>();
        var foundHeading = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');

            if (!foundHeading)
            {
                if (IsHeading(line, headingText))
                {
                    foundHeading = true;
                }
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("- "))
            {
                items.Add(trimmed[2..].Trim());
            }
            else if (trimmed.StartsWith('#'))
            {
                // Hit another heading, stop
                break;
            }
            // Skip blank lines and non-bullet content within section
        }

        return items;
    }

    /// <summary>
    /// Extracts a paragraph of text (non-heading, non-bullet lines) under a heading.
    /// </summary>
    /// <param name="markdownText">The full markdown text.</param>
    /// <param name="headingText">The heading to find.</param>
    /// <returns>The concatenated paragraph text.</returns>
    public static string ExtractParagraphUnderHeading(string markdownText, string headingText)
    {
        var lines = markdownText.Split('\n');
        var paragraphLines = new List<string>();
        var foundHeading = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');

            if (!foundHeading)
            {
                if (IsHeading(line, headingText))
                {
                    foundHeading = true;
                }
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith('#'))
            {
                break;
            }
            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith('>'))
            {
                paragraphLines.Add(trimmed);
            }
        }

        return string.Join(" ", paragraphLines).Trim();
    }

    /// <summary>
    /// Extracts items under a heading, supporting both bullet lists and paragraph text.
    /// If the section contains bullet items (<c>- </c> prefixed lines), returns those.
    /// Otherwise, extracts the paragraph text and splits it into individual sentences.
    /// </summary>
    /// <param name="markdownText">The full markdown text.</param>
    /// <param name="headingText">The heading to find.</param>
    /// <returns>List of items: either bullet entries or individual sentences.</returns>
    public static List<string> ExtractItemsUnderHeading(string markdownText, string headingText)
    {
        var bullets = ExtractBulletListUnderHeading(markdownText, headingText);
        if (bullets.Count > 0)
        {
            return bullets;
        }

        var paragraph = ExtractParagraphUnderHeading(markdownText, headingText);
        if (string.IsNullOrWhiteSpace(paragraph))
        {
            return [];
        }

        // Split paragraph into sentences at sentence-ending punctuation followed by whitespace and an uppercase letter
        var sentences = Regex.Split(paragraph, @"(?<=[.!?])\s+(?=[A-Z])")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();

        return sentences;
    }

    /// <summary>
    /// Extracts dated session log entries from the "Session Log" section.
    /// Each entry starts with a <c>### YYYY-MM-DD</c> sub-heading followed by bullet items.
    /// Entries are returned in file order (newest first if the file is written that way).
    /// </summary>
    /// <param name="markdownText">The full markdown document text.</param>
    /// <returns>A list of session log entries, each containing a date and bullet items.</returns>
    public static List<SessionLogEntry> ExtractSessionLogEntries(string markdownText)
    {
        var entries = new List<SessionLogEntry>();
        var lines = markdownText.Split('\n');
        var inSessionLog = false;
        SessionLogEntry? currentEntry = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            var trimmed = line.TrimStart();

            if (!inSessionLog)
            {
                if (IsHeading(line, ParserConstants.SectionSessionLog) && GetHeadingLevel(trimmed) == 2)
                {
                    inSessionLog = true;
                }
                continue;
            }

            // Stop at the next ## heading (different section)
            if (trimmed.StartsWith("## ") && !trimmed.StartsWith("### "))
            {
                break;
            }

            // New date entry: ### YYYY-MM-DD
            if (trimmed.StartsWith("### "))
            {
                currentEntry = new SessionLogEntry
                {
                    Date = trimmed[4..].Trim()
                };
                entries.Add(currentEntry);
                continue;
            }

            // Bullet items belonging to current entry
            if (currentEntry != null && trimmed.StartsWith("- "))
            {
                currentEntry.Items.Add(trimmed[2..].Trim());
            }
        }

        return entries;
    }

    /// <summary>
    /// Returns the heading level (number of leading # characters) for a line.
    /// </summary>
    private static int GetHeadingLevel(string trimmedLine)
    {
        var level = 0;
        foreach (var c in trimmedLine)
        {
            if (c == '#') level++;
            else break;
        }
        return level;
    }

    /// <summary>
    /// Checks if a line is a markdown heading matching the specified text.
    /// Requires that the heading is at level 2+ (## or deeper) for section headings,
    /// unless the heading text contains multiple words (which signals a document-level heading).
    /// </summary>
    private static bool IsHeading(string line, string headingText)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith('#'))
        {
            return false;
        }

        // Count heading level
        var level = 0;
        foreach (var c in trimmed)
        {
            if (c == '#') level++;
            else break;
        }

        var headingContent = trimmed.TrimStart('#').Trim();

        // Exact match at any level
        if (headingContent.Equals(headingText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // For contains/partial matches, only match ## or deeper to avoid
        // false positives on H1 title lines like "# Decision 004: Quality"
        if (level >= 2 && headingContent.Contains(headingText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses a markdown table row into an array of cell values.
    /// Strips leading/trailing pipes and whitespace from each cell.
    /// </summary>
    private static string[] ParseTableRow(string line)
    {
        // Split by | and remove empty first/last entries from leading/trailing |
        var parts = line.Split('|');
        var cells = new List<string>();

        for (var i = 1; i < parts.Length - 1; i++)
        {
            cells.Add(StripMarkdownFormatting(parts[i].Trim()));
        }

        return cells.ToArray();
    }

    /// <summary>
    /// Strips common markdown formatting (bold, strikethrough) from text while preserving content.
    /// </summary>
    private static string StripMarkdownFormatting(string text)
    {
        // Remove bold markers
        text = text.Replace("**", "");
        // Remove strikethrough markers
        text = text.Replace("~~", "");
        return text.Trim();
    }

    [GeneratedRegex(@"^\s*\|[\s\-:|]+\|")]
    private static partial Regex SeparatorPattern();
}
