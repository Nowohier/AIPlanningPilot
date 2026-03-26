using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DecisionParser"/>.
/// </summary>
[TestFixture]
public class DecisionParserTests
{
    private Mock<IFileSystemService> mockFs = null!;
    private DecisionParser _parser = null!;
    private string _sampleAdrContent = null!;

    [SetUp]
    public void SetUp()
    {
        mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        _parser = new DecisionParser(mockFs.Object);
        _sampleAdrContent = File.ReadAllText(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample-adr.md"));
    }

    [TearDown]
    public void TearDown()
    {
        mockFs.VerifyAll();
    }

    [Test]
    public void ParseAll_WhenAdrFilesExist_ShouldParseDecisionNumber()
    {
        // Arrange
        SetupDecisionsDirectory();

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result.Should().HaveCount(1);
        result[0].Number.Should().Be(4);
    }

    [Test]
    public void ParseAll_WhenAdrFilesExist_ShouldParseTitle()
    {
        // Arrange
        SetupDecisionsDirectory();

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result[0].Title.Should().Be("Quality standards");
    }

    [Test]
    public void ParseAll_WhenAdrFilesExist_ShouldParseMetadata()
    {
        // Arrange
        SetupDecisionsDirectory();

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result[0].Date.Should().Be("2026-03-05");
        result[0].Phase.Should().Be("Pre-Phase");
        result[0].Participants.Should().Contain("Chris");
    }

    [Test]
    public void ParseAll_WhenAdrFilesExist_ShouldParseContextAndDecision()
    {
        // Arrange
        SetupDecisionsDirectory();

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result[0].Context.Should().Contain("hand-written code");
        result[0].DecisionText.Should().Contain("unit tests");
    }

    [Test]
    public void ParseAll_WhenDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        mockFs.Setup(fs => fs.FileExists(@"C:\decisions\INDEX.md")).Returns(false);
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\decisions")).Returns(false);

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ParseAll_WhenIndexMdExists_ShouldPopulateAffectsField()
    {
        // Arrange
        var indexContent = """
            # Decisions Index

            | # | Title | Date | Affects |
            |---|-------|------|---------|
            | 4 | Quality standards | 2026-03-05 | Frontend, Backend |
            """;

        mockFs.Setup(fs => fs.FileExists(@"C:\decisions\INDEX.md")).Returns(true);
        mockFs.Setup(fs => fs.ReadAllText(@"C:\decisions\INDEX.md")).Returns(indexContent);
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\decisions")).Returns(true);
        mockFs.Setup(fs => fs.GetDirectoryTree(@"C:\decisions", false))
            .Returns(
            [
                new FileTreeNode
                {
                    Name = "004-quality-standards.md",
                    FullPath = @"C:\decisions\004-quality-standards.md",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                }
            ]);
        mockFs.Setup(fs => fs.ReadAllText(@"C:\decisions\004-quality-standards.md"))
            .Returns(_sampleAdrContent);

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result.Should().HaveCount(1);
        result[0].Affects.Should().Be("Frontend, Backend");
    }

    [Test]
    public void ParseAll_WhenAdrFileHasInvalidTitle_ShouldSkipFile()
    {
        // Arrange
        var invalidAdrContent = """
            # Some random heading without decision pattern

            > **Date**: 2026-03-05

            ## Context

            This file does not follow the ADR title convention.
            """;

        mockFs.Setup(fs => fs.FileExists(@"C:\decisions\INDEX.md")).Returns(false);
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\decisions")).Returns(true);
        mockFs.Setup(fs => fs.GetDirectoryTree(@"C:\decisions", false))
            .Returns(
            [
                new FileTreeNode
                {
                    Name = "005-invalid-title.md",
                    FullPath = @"C:\decisions\005-invalid-title.md",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                }
            ]);
        mockFs.Setup(fs => fs.ReadAllText(@"C:\decisions\005-invalid-title.md"))
            .Returns(invalidAdrContent);

        // Act
        var result = _parser.ParseAll(@"C:\decisions");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Constructor_WhenNullFileSystem_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DecisionParser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Sets up mock for a decisions directory with one ADR file and no INDEX.md.
    /// </summary>
    private void SetupDecisionsDirectory()
    {
        mockFs.Setup(fs => fs.FileExists(@"C:\decisions\INDEX.md")).Returns(false);
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\decisions")).Returns(true);
        mockFs.Setup(fs => fs.GetDirectoryTree(@"C:\decisions", false))
            .Returns(
            [
                new FileTreeNode
                {
                    Name = "004-quality-standards.md",
                    FullPath = @"C:\decisions\004-quality-standards.md",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                }
            ]);
        mockFs.Setup(fs => fs.ReadAllText(@"C:\decisions\004-quality-standards.md"))
            .Returns(_sampleAdrContent);
    }
}
