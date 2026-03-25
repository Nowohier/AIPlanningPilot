using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FileSystemService"/>.
/// Uses real file system operations against TestData directory and temp paths.
/// </summary>
[TestFixture]
public class FileSystemServiceTests
{
    private FileSystemService _sut = null!;
    private string _testDataPath = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new FileSystemService();
        _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
    }

    [Test]
    public void GetDirectoryTree_WhenDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDataPath, "does-not-exist-dir");

        // Act
        var result = _sut.GetDirectoryTree(nonExistentPath);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ReadAllText_WhenFileExists_ShouldReturnContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "sample-state.md");

        // Act
        var content = _sut.ReadAllText(filePath);

        // Assert
        content.Should().NotBeNullOrWhiteSpace();
        content.Should().Contain("STATE.md");
    }

    [Test]
    public void FileExists_WhenFileExists_ShouldReturnTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "sample-state.md");

        // Act
        var exists = _sut.FileExists(filePath);

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public void FileExists_WhenFileMissing_ShouldReturnFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "nonexistent-file.md");

        // Act
        var exists = _sut.FileExists(filePath);

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public void DirectoryExists_WhenDirectoryExists_ShouldReturnTrue()
    {
        // Arrange -- TestData directory itself exists

        // Act
        var exists = _sut.DirectoryExists(_testDataPath);

        // Assert
        exists.Should().BeTrue();
    }
}
