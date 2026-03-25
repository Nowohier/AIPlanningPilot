using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="HandoverParser"/>.
/// </summary>
[TestFixture]
public class HandoverParserTests
{
    private Mock<IFileSystemService> _mockFs = null!;
    private HandoverParser _parser = null!;
    private string _sampleContent = null!;

    [SetUp]
    public void SetUp()
    {
        _mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        _parser = new HandoverParser(_mockFs.Object);
        _sampleContent = File.ReadAllText(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample-handover.md"));
    }

    [TearDown]
    public void TearDown()
    {
        _mockFs.VerifyAll();
    }

    [Test]
    public void ParseAll_WhenHandoverFilesExist_ShouldParseDevName()
    {
        // Arrange
        SetupHandoverDirectory("chris");

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result.Should().HaveCount(1);
        result[0].DeveloperName.Should().Be("chris");
    }

    [Test]
    public void ParseAll_WhenHandoverFilesExist_ShouldParseLastUpdated()
    {
        // Arrange
        SetupHandoverDirectory("chris");

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result[0].LastUpdated.Should().Be("2026-03-23");
    }

    [Test]
    public void ParseAll_WhenHandoverFilesExist_ShouldParseForNextSession()
    {
        // Arrange
        SetupHandoverDirectory("chris");

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result[0].ForNextSession.Should().HaveCount(3);
        result[0].ForNextSession[0].Should().Contain("CLAUDE.md");
    }

    [Test]
    public void ParseAll_WhenHandoverFilesExist_ShouldParseOpenThreads()
    {
        // Arrange
        SetupHandoverDirectory("chris");

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result[0].OpenThreads.Should().HaveCount(2);
    }

    [Test]
    public void ParseAll_WhenHandoverFilesExist_ShouldParseFromLastSession()
    {
        // Arrange
        SetupHandoverDirectory("chris");

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result[0].FromLastSession.Should().NotBeEmpty();
        result[0].FromLastSession[0].Should().Contain("AI tooling");
    }

    [Test]
    public void ParseAll_WhenDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        _mockFs.Setup(fs => fs.DirectoryExists(@"C:\handovers")).Returns(false);

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ParseAll_WhenNonMatchingFilesExist_ShouldIgnoreThem()
    {
        // Arrange
        _mockFs.Setup(fs => fs.DirectoryExists(@"C:\handovers")).Returns(true);
        _mockFs.Setup(fs => fs.GetDirectoryTree(@"C:\handovers", false))
            .Returns(
            [
                new FileTreeNode
                {
                    Name = "handover-chris.md",
                    FullPath = @"C:\handovers\handover-chris.md",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                },
                new FileTreeNode
                {
                    Name = "README.md",
                    FullPath = @"C:\handovers\README.md",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                },
                new FileTreeNode
                {
                    Name = "notes.txt",
                    FullPath = @"C:\handovers\notes.txt",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                }
            ]);
        _mockFs.Setup(fs => fs.ReadAllText(@"C:\handovers\handover-chris.md"))
            .Returns(_sampleContent);

        // Act
        var result = _parser.ParseAll(@"C:\handovers");

        // Assert
        result.Should().HaveCount(1);
        result[0].DeveloperName.Should().Be("chris");
    }

    [Test]
    public void Constructor_WhenNullFileSystem_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new HandoverParser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Sets up mock for a handovers directory containing a single handover file.
    /// </summary>
    private void SetupHandoverDirectory(string devName)
    {
        _mockFs.Setup(fs => fs.DirectoryExists(@"C:\handovers")).Returns(true);
        _mockFs.Setup(fs => fs.GetDirectoryTree(@"C:\handovers", false))
            .Returns(
            [
                new FileTreeNode
                {
                    Name = $"handover-{devName}.md",
                    FullPath = $@"C:\handovers\handover-{devName}.md",
                    IsDirectory = false,
                    LastModified = DateTime.UtcNow
                }
            ]);
        _mockFs.Setup(fs => fs.ReadAllText($@"C:\handovers\handover-{devName}.md"))
            .Returns(_sampleContent);
    }
}
