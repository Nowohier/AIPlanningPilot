using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ConfigurationService"/>.
/// </summary>
[TestFixture]
public class ConfigurationServiceTests
{
    private Mock<IFileSystemService> mockFileSystem = null!;

    [SetUp]
    public void SetUp()
    {
        mockFileSystem = new Mock<IFileSystemService>(MockBehavior.Strict);
    }

    [TearDown]
    public void TearDown()
    {
        mockFileSystem.VerifyAll();
    }

    [Test]
    public void Constructor_WhenValidPath_ShouldStoreRestructuringRootPath()
    {
        // Arrange
        var path = @"C:\restructuring";
        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var service = new ConfigurationService(path, mockFileSystem.Object);

        // Assert
        service.RestructuringRootPath.Should().Be(path);
    }

    [Test]
    public void Constructor_WhenNullPath_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ConfigurationService(null!, mockFileSystem.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("restructuringRootPath");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ConfigurationService(@"C:\root", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void ProjectName_WhenProjectJsonExists_ShouldReturnProjectName()
    {
        // Arrange
        var path = @"C:\restructuring";
        var configPath = @"C:\restructuring\main\project.json";
        mockFileSystem.Setup(fs => fs.FileExists(configPath)).Returns(true);
        mockFileSystem.Setup(fs => fs.ReadAllText(configPath)).Returns("""{"projectName": "MyProject"}""");

        // Act
        var service = new ConfigurationService(path, mockFileSystem.Object);

        // Assert
        service.ProjectName.Should().Be("MyProject");
    }

    [Test]
    public void ProjectName_WhenProjectJsonMissing_ShouldReturnEmpty()
    {
        // Arrange
        var path = @"C:\restructuring";
        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var service = new ConfigurationService(path, mockFileSystem.Object);

        // Assert
        service.ProjectName.Should().BeEmpty();
    }
}
