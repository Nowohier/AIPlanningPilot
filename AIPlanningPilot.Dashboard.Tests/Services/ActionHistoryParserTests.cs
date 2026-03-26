using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ActionHistoryParser"/>.
/// </summary>
[TestFixture]
public class ActionHistoryParserTests
{
    private Mock<IFileSystemService> mockFs = null!;
    private ActionHistoryParser _parser = null!;
    private string _sampleContent = null!;

    [SetUp]
    public void SetUp()
    {
        mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        _parser = new ActionHistoryParser(mockFs.Object);
        _sampleContent = File.ReadAllText(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample-completed-actions.md"));
    }

    [TearDown]
    public void TearDown()
    {
        mockFs.VerifyAll();
    }

    [Test]
    public void Parse_WhenActionsPresent_ShouldParseAllRows()
    {
        // Arrange
        mockFs.Setup(fs => fs.ReadAllText("actions.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("actions.md");

        // Assert
        result.Should().HaveCount(3);
    }

    [Test]
    public void Parse_WhenActionsPresent_ShouldParseFieldsCorrectly()
    {
        // Arrange
        mockFs.Setup(fs => fs.ReadAllText("actions.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("actions.md");

        // Assert
        result[0].Number.Should().Be(1);
        result[0].Description.Should().Contain("PLAN.md questions");
        result[0].Owner.Should().Be("Chris");
        result[0].CompletedDate.Should().Be("2026-03-04");
        result[0].Notes.Should().Contain("Decisions 001-003");
    }

    [Test]
    public void Parse_WhenActionsPresent_ShouldParseMultipleOwners()
    {
        // Arrange
        mockFs.Setup(fs => fs.ReadAllText("actions.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("actions.md");

        // Assert
        result[0].Owner.Should().Be("Chris");
        result[1].Owner.Should().Be("Claude");
    }

    [Test]
    public void Parse_WhenTableUnderHeadingEmpty_ShouldFallbackToFirstTable()
    {
        // Arrange
        var content = """
            Some introduction text.

            | # | Action | Owner | Completed | Notes |
            |---|--------|-------|-----------|-------|
            | 7 | Fallback action | Alice | 2026-03-10 | Via first table |
            """;
        mockFs.Setup(fs => fs.ReadAllText("actions.md")).Returns(content);

        // Act
        var result = _parser.Parse("actions.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Number.Should().Be(7);
        result[0].Description.Should().Be("Fallback action");
        result[0].Owner.Should().Be("Alice");
    }

    [Test]
    public void Parse_WhenRowHasFewerThanFiveColumns_ShouldSkipRow()
    {
        // Arrange
        var content = """
            # Completed Actions Archive

            | # | Action | Owner | Completed | Notes |
            |---|--------|-------|-----------|-------|
            | 1 | Valid row | Chris | 2026-03-04 | OK |
            | 2 | Short row | Bob |
            | 3 | Another valid | Eve | 2026-03-05 | Done |
            """;
        mockFs.Setup(fs => fs.ReadAllText("actions.md")).Returns(content);

        // Act
        var result = _parser.Parse("actions.md");

        // Assert
        result.Should().HaveCount(2);
        result[0].Number.Should().Be(1);
        result[1].Number.Should().Be(3);
    }

    [Test]
    public void Parse_WhenNumberIsNonNumeric_ShouldDefaultToZero()
    {
        // Arrange
        var content = """
            # Completed Actions Archive

            | # | Action | Owner | Completed | Notes |
            |---|--------|-------|-----------|-------|
            | abc | Non-numeric action | Chris | 2026-03-04 | Test |
            """;
        mockFs.Setup(fs => fs.ReadAllText("actions.md")).Returns(content);

        // Act
        var result = _parser.Parse("actions.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Number.Should().Be(0);
        result[0].Description.Should().Be("Non-numeric action");
    }

    [Test]
    public void Constructor_WhenNullFileSystem_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ActionHistoryParser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
