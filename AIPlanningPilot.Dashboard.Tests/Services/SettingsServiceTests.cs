using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="SettingsService"/>.
/// </summary>
[TestFixture]
public class SettingsServiceTests
{
    private Mock<IFileSystemService> mockFs = null!;
    private SettingsService sut = null!;

    private static readonly string ExpectedSettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AIPlanningPilot.Dashboard");

    private static readonly string ExpectedSettingsFilePath = Path.Combine(
        ExpectedSettingsDirectory, "settings.json");

    /// <summary>
    /// Sets up mocks and the system under test before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        sut = new SettingsService(mockFs.Object);
    }

    /// <summary>
    /// Verifies that all expected mock calls were made.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        mockFs.VerifyAll();
    }

    /// <summary>
    /// Passing null for the file system service should throw an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Act
        var act = () => new SettingsService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fileSystemService");
    }

    /// <summary>
    /// When the settings file does not exist, Load should keep the default theme name.
    /// </summary>
    [Test]
    public void Load_WhenSettingsFileDoesNotExist_ShouldUseDefaults()
    {
        // Arrange
        mockFs.Setup(fs => fs.FileExists(ExpectedSettingsFilePath)).Returns(false);

        // Act
        sut.Load();

        // Assert
        sut.SelectedThemeName.Should().Be("GitHub Light");
    }

    /// <summary>
    /// When the settings file exists with valid JSON, Load should deserialize the theme name.
    /// </summary>
    [Test]
    public void Load_WhenSettingsFileExists_ShouldDeserializeThemeName()
    {
        // Arrange
        var json = """{ "SelectedThemeName": "Dracula" }""";
        mockFs.Setup(fs => fs.FileExists(ExpectedSettingsFilePath)).Returns(true);
        mockFs.Setup(fs => fs.ReadAllText(ExpectedSettingsFilePath)).Returns(json);

        // Act
        sut.Load();

        // Assert
        sut.SelectedThemeName.Should().Be("Dracula");
    }

    /// <summary>
    /// When the settings file contains invalid JSON, Load should fall back to defaults.
    /// </summary>
    [Test]
    public void Load_WhenSettingsFileContainsInvalidJson_ShouldUseDefaults()
    {
        // Arrange
        mockFs.Setup(fs => fs.FileExists(ExpectedSettingsFilePath)).Returns(true);
        mockFs.Setup(fs => fs.ReadAllText(ExpectedSettingsFilePath)).Returns("not valid json {{{");

        // Act
        sut.Load();

        // Assert
        sut.SelectedThemeName.Should().Be("GitHub Light");
    }

    /// <summary>
    /// Save should create the settings directory and write the serialized JSON.
    /// </summary>
    [Test]
    public void Save_WhenCalled_ShouldCreateDirectoryAndWriteJson()
    {
        // Arrange
        sut.SelectedThemeName = "Monokai";
        mockFs.Setup(fs => fs.CreateDirectory(ExpectedSettingsDirectory));
        mockFs.Setup(fs => fs.WriteAllText(ExpectedSettingsFilePath, It.Is<string>(
            json => json.Contains("\"SelectedThemeName\"") && json.Contains("Monokai"))));

        // Act
        sut.Save();

        // Assert
        mockFs.Verify(fs => fs.CreateDirectory(ExpectedSettingsDirectory), Times.Once);
        mockFs.Verify(fs => fs.WriteAllText(ExpectedSettingsFilePath, It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Setting SelectedThemeName should return the new value on subsequent reads.
    /// </summary>
    [Test]
    public void SelectedThemeName_WhenSet_ShouldReturnNewValue()
    {
        // Arrange
        var newTheme = "Solarized Dark";

        // Act
        sut.SelectedThemeName = newTheme;

        // Assert
        sut.SelectedThemeName.Should().Be("Solarized Dark");
    }
}
