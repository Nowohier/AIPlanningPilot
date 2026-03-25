using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="NavigationService"/>.
/// </summary>
[TestFixture]
public class NavigationServiceTests
{
    [Test]
    public void NavigateToFile_WhenSubscribed_ShouldRaiseFileSelectedEvent()
    {
        // Arrange
        var service = new NavigationService();
        string? receivedPath = null;
        service.FileSelected += (_, path) => receivedPath = path;

        // Act
        service.NavigateToFile(@"C:\restructuring\main\STATE.md");

        // Assert
        receivedPath.Should().Be(@"C:\restructuring\main\STATE.md");
    }

    [Test]
    public void NavigateToFile_WhenNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var service = new NavigationService();

        // Act
        var act = () => service.NavigateToFile(@"C:\some\file.md");

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void NavigateToFile_WhenMultipleSubscribers_ShouldNotifyAll()
    {
        // Arrange
        var service = new NavigationService();
        string? path1 = null;
        string? path2 = null;
        service.FileSelected += (_, path) => path1 = path;
        service.FileSelected += (_, path) => path2 = path;

        // Act
        service.NavigateToFile(@"C:\test.md");

        // Assert
        path1.Should().Be(@"C:\test.md");
        path2.Should().Be(@"C:\test.md");
    }
}
