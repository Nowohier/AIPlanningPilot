using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SearchService"/>.
/// Uses real file system operations against TestData directory.
/// </summary>
[TestFixture]
public class SearchServiceTests
{
    private SearchService _sut = null!;
    private string _testDataPath = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new SearchService(new FileSystemService());
        _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
    }

    [Test]
    public void Search_WhenQueryMatchesContent_ShouldReturnResults()
    {
        // Arrange
        var query = "Morning Briefing";

        // Act
        var results = _sut.Search(query, _testDataPath);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.FileName == "sample-state.md");
    }

    [Test]
    public void Search_WhenQueryIsEmpty_ShouldReturnEmpty()
    {
        // Arrange
        var query = "";

        // Act
        var results = _sut.Search(query, _testDataPath);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Search_WhenDirectoryDoesNotExist_ShouldReturnEmpty()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDataPath, "does-not-exist");

        // Act
        var results = _sut.Search("anything", nonExistentPath);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Search_WhenNoMatches_ShouldReturnEmpty()
    {
        // Arrange
        var query = "xyzzy_no_match_expected_12345";

        // Act
        var results = _sut.Search(query, _testDataPath);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void Search_WhenMultipleFilesMatch_ShouldReturnAllMatches()
    {
        // Arrange -- "##" appears in all markdown files as a heading marker
        var query = "##";

        // Act
        var results = _sut.Search(query, _testDataPath);

        // Assert
        var distinctFiles = results.Select(r => r.FileName).Distinct().ToList();
        distinctFiles.Should().HaveCountGreaterThan(1,
            "multiple TestData .md files contain markdown headings");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new SearchService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
