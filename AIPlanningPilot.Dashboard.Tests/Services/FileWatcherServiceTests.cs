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
    private FileWatcherService sut = null!;
    private string testDataPath = null!;

    [SetUp]
    public void SetUp()
    {
        sut = new FileWatcherService(action => action());
        testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
    }

    [TearDown]
    public void TearDown()
    {
        sut.Dispose();
    }

    [Test]
    public void Start_WhenValidPath_ShouldSetIsWatchingTrue()
    {
        // Arrange -- TestData directory exists

        // Act
        sut.Start(testDataPath);

        // Assert
        sut.IsWatching.Should().BeTrue();
    }

    [Test]
    public void Stop_WhenWatching_ShouldSetIsWatchingFalse()
    {
        // Arrange
        sut.Start(testDataPath);
        sut.IsWatching.Should().BeTrue("precondition: watcher should be active");

        // Act
        sut.Stop();

        // Assert
        sut.IsWatching.Should().BeFalse();
    }

    [Test]
    public void Start_WhenDirectoryDoesNotExist_ShouldNotWatch()
    {
        // Arrange
        var nonExistentPath = Path.Combine(testDataPath, "does-not-exist-watcher-dir");

        // Act
        sut.Start(nonExistentPath);

        // Assert
        sut.IsWatching.Should().BeFalse();
    }

    [Test]
    public void Dispose_WhenCalled_ShouldStopWatching()
    {
        // Arrange
        sut.Start(testDataPath);
        sut.IsWatching.Should().BeTrue("precondition: watcher should be active");

        // Act
        sut.Dispose();

        // Assert
        sut.IsWatching.Should().BeFalse();
    }
}
