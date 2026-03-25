using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ConfigurationService"/>.
/// </summary>
[TestFixture]
public class ConfigurationServiceTests
{
    [Test]
    public void Constructor_WhenValidPath_ShouldStoreRestructuringRootPath()
    {
        // Arrange
        var path = @"C:\restructuring";

        // Act
        var service = new ConfigurationService(path);

        // Assert
        service.RestructuringRootPath.Should().Be(path);
    }

    [Test]
    public void Constructor_WhenNullPath_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ConfigurationService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("restructuringRootPath");
    }
}
