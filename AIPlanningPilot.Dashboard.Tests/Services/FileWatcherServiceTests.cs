using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FileWatcherService"/>.
/// Tests state management only; the <see cref="System.Windows.Threading.DispatcherTimer"/>
/// debounce logic cannot be exercised without a WPF dispatcher.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class FileWatcherServiceTests
{
    private FileWatcherService _sut = null!;
    private string _testDataPath = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new FileWatcherService();
        _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public void Start_WhenValidPath_ShouldSetIsWatchingTrue()
    {
        // Arrange -- TestData directory exists

        // Act
        _sut.Start(_testDataPath);

        // Assert
        _sut.IsWatching.Should().BeTrue();
    }

    [Test]
    public void Stop_WhenWatching_ShouldSetIsWatchingFalse()
    {
        // Arrange
        _sut.Start(_testDataPath);
        _sut.IsWatching.Should().BeTrue("precondition: watcher should be active");

        // Act
        _sut.Stop();

        // Assert
        _sut.IsWatching.Should().BeFalse();
    }

    [Test]
    public void Start_WhenDirectoryDoesNotExist_ShouldNotWatch()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDataPath, "does-not-exist-watcher-dir");

        // Act
        _sut.Start(nonExistentPath);

        // Assert
        _sut.IsWatching.Should().BeFalse();
    }

    [Test]
    public void Dispose_WhenCalled_ShouldStopWatching()
    {
        // Arrange
        _sut.Start(_testDataPath);
        _sut.IsWatching.Should().BeTrue("precondition: watcher should be active");

        // Act
        _sut.Dispose();

        // Assert
        _sut.IsWatching.Should().BeFalse();
    }
}
