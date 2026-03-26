using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="MarkdownTableParser"/>.
/// </summary>
[TestFixture]
public class MarkdownTableParserTests
{
    [Test]
    public void ExtractTableUnderHeading_WhenTableExists_ShouldReturnRows()
    {
        // Arrange
        var md = """
            ## My Table

            | A | B | C |
            |---|---|---|
            | 1 | 2 | 3 |
            | 4 | 5 | 6 |
            """;

        // Act
        var result = MarkdownTableParser.ExtractTableUnderHeading(md, "My Table");

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new[] { "1", "2", "3" });
        result[1].Should().BeEquivalentTo(new[] { "4", "5", "6" });
    }

    [Test]
    public void ExtractTableUnderHeading_WhenBoldFormatting_ShouldStripBold()
    {
        // Arrange
        var md = """
            ## Status

            | Name | Value |
            |------|-------|
            | **Phase** | **Done** |
            """;

        // Act
        var result = MarkdownTableParser.ExtractTableUnderHeading(md, "Status");

        // Assert
        result.Should().HaveCount(1);
        result[0][0].Should().Be("Phase");
        result[0][1].Should().Be("Done");
    }

    [Test]
    public void ExtractTableUnderHeading_WhenStrikethrough_ShouldStripStrikethrough()
    {
        // Arrange
        var md = """
            ## Actions

            | # | Action | Status |
            |---|--------|--------|
            | 1 | ~~Old task~~ | Done |
            """;

        // Act
        var result = MarkdownTableParser.ExtractTableUnderHeading(md, "Actions");

        // Assert
        result[0][1].Should().Be("Old task");
    }

    [Test]
    public void ExtractTableUnderHeading_WhenHeadingNotFound_ShouldReturnEmpty()
    {
        // Arrange
        var md = "## Other Heading\n\nSome text.";

        // Act
        var result = MarkdownTableParser.ExtractTableUnderHeading(md, "Missing");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ExtractTableUnderHeading_WhenTableStopsAtNextHeading_ShouldOnlyReturnFirstTable()
    {
        // Arrange
        var md = """
            ## First

            | A | B |
            |---|---|
            | 1 | 2 |

            ## Second

            | C | D |
            |---|---|
            | 3 | 4 |
            """;

        // Act
        var result = MarkdownTableParser.ExtractTableUnderHeading(md, "First");

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new[] { "1", "2" });
    }

    [Test]
    public void ExtractBlockquoteValue_WhenKeyExists_ShouldReturnValue()
    {
        // Arrange
        var md = "> **Phase**: Phase 1\n> **Day**: 3";

        // Act
        var result = MarkdownTableParser.ExtractBlockquoteValue(md, "Phase");

        // Assert
        result.Should().Be("Phase 1");
    }

    [Test]
    public void ExtractBlockquoteValue_WhenKeyMissing_ShouldReturnEmpty()
    {
        // Arrange
        var md = "> **Phase**: Phase 1";

        // Act
        var result = MarkdownTableParser.ExtractBlockquoteValue(md, "Missing");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ExtractBulletListUnderHeading_WhenBulletsExist_ShouldReturnItems()
    {
        // Arrange
        var md = """
            ## My List

            - First item
            - Second item
            - Third item

            ## Other
            """;

        // Act
        var result = MarkdownTableParser.ExtractBulletListUnderHeading(md, "My List");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("First item");
        result[2].Should().Be("Third item");
    }

    [Test]
    public void ExtractParagraphUnderHeading_WhenTextExists_ShouldReturnJoinedText()
    {
        // Arrange
        var md = """
            ## Context

            This is the first line.
            And the second line.

            ## Next
            """;

        // Act
        var result = MarkdownTableParser.ExtractParagraphUnderHeading(md, "Context");

        // Assert
        result.Should().Contain("first line");
        result.Should().Contain("second line");
    }

    [Test]
    public void ExtractFirstTable_WhenTableExists_ShouldReturnDataRows()
    {
        // Arrange
        var content = """
            Some introductory text.

            | Name | Value |
            |------|-------|
            | Alpha | 10 |
            | Beta | 20 |

            More text after the table.
            """;

        // Act
        var result = MarkdownTableParser.ExtractFirstTable(content);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new[] { "Alpha", "10" });
        result[1].Should().BeEquivalentTo(new[] { "Beta", "20" });
    }

    [Test]
    public void ExtractFirstTable_WhenNoTable_ShouldReturnEmpty()
    {
        // Arrange
        var content = """
            # Just a heading

            Some paragraph text without any table.

            - A bullet list
            - Another bullet
            """;

        // Act
        var result = MarkdownTableParser.ExtractFirstTable(content);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ExtractItemsUnderHeading_WhenBulletList_ShouldReturnBullets()
    {
        // Arrange
        var md = """
            ## Tasks

            - Write unit tests
            - Review pull request
            - Update documentation

            ## Other
            """;

        // Act
        var result = MarkdownTableParser.ExtractItemsUnderHeading(md, "Tasks");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("Write unit tests");
        result[1].Should().Be("Review pull request");
        result[2].Should().Be("Update documentation");
    }

    [Test]
    public void ExtractItemsUnderHeading_WhenParagraph_ShouldSplitSentences()
    {
        // Arrange
        var md = """
            ## Summary

            First sentence here. Second sentence follows. Third one ends it.

            ## Next
            """;

        // Act
        var result = MarkdownTableParser.ExtractItemsUnderHeading(md, "Summary");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("First sentence here.");
        result[1].Should().Be("Second sentence follows.");
        result[2].Should().Be("Third one ends it.");
    }

    [Test]
    public void ExtractItemsUnderHeading_WhenEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var md = """
            ## Empty Section

            ## Next Section

            - Some content here
            """;

        // Act
        var result = MarkdownTableParser.ExtractItemsUnderHeading(md, "Empty Section");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ExtractSessionLogEntries_WhenSessionLogExists_ShouldParseDates()
    {
        // Arrange
        var md = """
            ## Session Log

            ### 2026-03-26
            - Did some work
            - Fixed a bug

            ### 2026-03-25
            - Initial setup
            """;

        // Act
        var result = MarkdownTableParser.ExtractSessionLogEntries(md);

        // Assert
        result.Should().HaveCount(2);
        result[0].Date.Should().Be("2026-03-26");
        result[1].Date.Should().Be("2026-03-25");
    }

    [Test]
    public void ExtractSessionLogEntries_WhenSessionLogExists_ShouldParseItems()
    {
        // Arrange
        var md = """
            ## Session Log

            ### 2026-03-26
            - Did some work
            - Fixed a bug
            - Files: foo.cs, bar.cs

            ### 2026-03-25
            - Initial setup
            """;

        // Act
        var result = MarkdownTableParser.ExtractSessionLogEntries(md);

        // Assert
        result[0].Items.Should().HaveCount(3);
        result[0].Items[0].Should().Be("Did some work");
        result[0].Items[1].Should().Be("Fixed a bug");
        result[0].Items[2].Should().Be("Files: foo.cs, bar.cs");
        result[1].Items.Should().HaveCount(1);
        result[1].Items[0].Should().Be("Initial setup");
    }

    [Test]
    public void ExtractSessionLogEntries_WhenNoSessionLog_ShouldReturnEmpty()
    {
        // Arrange
        var md = """
            ## For Next Session
            - Some items

            ## From Last Session
            Some text.
            """;

        // Act
        var result = MarkdownTableParser.ExtractSessionLogEntries(md);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ExtractSessionLogEntries_WhenEmptySessionLog_ShouldReturnEmpty()
    {
        // Arrange
        var md = """
            ## Session Log

            _No sessions recorded yet._
            """;

        // Act
        var result = MarkdownTableParser.ExtractSessionLogEntries(md);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ExtractSessionLogEntries_WhenFollowedByAnotherSection_ShouldStopAtNextH2()
    {
        // Arrange
        var md = """
            ## Session Log

            ### 2026-03-26
            - Did some work

            ## Some Other Section
            - Not a session log entry
            """;

        // Act
        var result = MarkdownTableParser.ExtractSessionLogEntries(md);

        // Assert
        result.Should().HaveCount(1);
        result[0].Items.Should().HaveCount(1);
    }

    [Test]
    public void ExtractTableUnderHeading_WhenHeaderOnlyNoDataRows_ShouldReturnEmpty()
    {
        // Arrange
        var md = """
            ## My Table

            | A | B | C |
            |---|---|---|

            ## Next Section
            """;

        // Act
        var result = MarkdownTableParser.ExtractTableUnderHeading(md, "My Table");

        // Assert
        result.Should().BeEmpty();
    }
}
