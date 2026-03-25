using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="SettingsViewModel"/>.
/// </summary>
[TestFixture]
public class SettingsViewModelTests
{
    private Mock<ISettingsService> _mockSettingsService = null!;
    private Mock<IMarkdownRenderer> _mockMarkdownRenderer = null!;

    /// <summary>
    /// Initializes mock dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockSettingsService = new Mock<ISettingsService>(MockBehavior.Strict);
        _mockMarkdownRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _mockSettingsService.VerifyAll();
        _mockMarkdownRenderer.VerifyAll();
    }

    /// <summary>
    /// Verifies that the constructor initializes SelectedThemeName from the renderer.
    /// </summary>
    [Test]
    public void Constructor_WhenCalled_ShouldInitializeFromRenderer()
    {
        // Arrange
        _mockMarkdownRenderer.Setup(r => r.SelectedThemeName).Returns("GitHub Dark");
        _mockMarkdownRenderer.Setup(r => r.AvailableThemes).Returns(new[] { "GitHub Dark", "GitHub Light" });

        // Act
        var vm = CreateViewModel();

        // Assert
        vm.SelectedThemeName.Should().Be("GitHub Dark");
        vm.AvailableThemes.Should().BeEquivalentTo(new[] { "GitHub Dark", "GitHub Light" });
    }

    /// <summary>
    /// Verifies that the constructor throws when settingsService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullSettingsService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new SettingsViewModel(null!, _mockMarkdownRenderer.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("settingsService");
    }

    /// <summary>
    /// Verifies that the constructor throws when markdownRenderer is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullMarkdownRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new SettingsViewModel(_mockSettingsService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("markdownRenderer");
    }

    /// <summary>
    /// Verifies that Save applies the selected theme to the markdown renderer.
    /// </summary>
    [Test]
    public void Save_WhenCalled_ShouldApplyThemeToRenderer()
    {
        // Arrange
        _mockMarkdownRenderer.Setup(r => r.SelectedThemeName).Returns("GitHub Dark");
        _mockMarkdownRenderer.SetupSet(r => r.SelectedThemeName = "GitHub Light");
        _mockSettingsService.SetupSet(s => s.SelectedThemeName = "GitHub Light");
        _mockSettingsService.Setup(s => s.Save());

        var vm = CreateViewModel();
        vm.SelectedThemeName = "GitHub Light";

        // Act
        vm.Save();

        // Assert
        _mockMarkdownRenderer.VerifySet(r => r.SelectedThemeName = "GitHub Light", Times.Once());
    }

    /// <summary>
    /// Verifies that Save persists the selected theme to the settings service.
    /// </summary>
    [Test]
    public void Save_WhenCalled_ShouldPersistToSettingsService()
    {
        // Arrange
        _mockMarkdownRenderer.Setup(r => r.SelectedThemeName).Returns("GitHub Dark");
        _mockMarkdownRenderer.SetupSet(r => r.SelectedThemeName = "GitHub Light");
        _mockSettingsService.SetupSet(s => s.SelectedThemeName = "GitHub Light");
        _mockSettingsService.Setup(s => s.Save());

        var vm = CreateViewModel();
        vm.SelectedThemeName = "GitHub Light";

        // Act
        vm.Save();

        // Assert
        _mockSettingsService.VerifySet(s => s.SelectedThemeName = "GitHub Light", Times.Once());
        _mockSettingsService.Verify(s => s.Save(), Times.Once());
    }

    /// <summary>
    /// Verifies that Save raises the CloseRequested event with true.
    /// </summary>
    [Test]
    public void Save_WhenCalled_ShouldRaiseCloseRequested()
    {
        // Arrange
        _mockMarkdownRenderer.Setup(r => r.SelectedThemeName).Returns("GitHub Dark");
        _mockMarkdownRenderer.SetupSet(r => r.SelectedThemeName = "GitHub Dark");
        _mockSettingsService.SetupSet(s => s.SelectedThemeName = "GitHub Dark");
        _mockSettingsService.Setup(s => s.Save());

        var vm = CreateViewModel();
        bool? closeResult = null;
        vm.CloseRequested += result => closeResult = result;

        // Act
        vm.Save();

        // Assert
        closeResult.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Cancel raises the CloseRequested event with false.
    /// </summary>
    [Test]
    public void Cancel_WhenCalled_ShouldRaiseCloseRequestedWithFalse()
    {
        // Arrange
        _mockMarkdownRenderer.Setup(r => r.SelectedThemeName).Returns("GitHub Dark");

        var vm = CreateViewModel();
        bool? closeResult = null;
        vm.CloseRequested += result => closeResult = result;

        // Act
        vm.Cancel();

        // Assert
        closeResult.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Cancel does not save settings to the settings service.
    /// </summary>
    [Test]
    public void Cancel_WhenCalled_ShouldNotSaveSettings()
    {
        // Arrange
        _mockMarkdownRenderer.Setup(r => r.SelectedThemeName).Returns("GitHub Dark");

        var vm = CreateViewModel();

        // Act
        vm.Cancel();

        // Assert
        _mockSettingsService.Verify(s => s.Save(), Times.Never());
    }

    /// <summary>
    /// Creates a <see cref="SettingsViewModel"/> with all mock dependencies.
    /// </summary>
    private SettingsViewModel CreateViewModel()
    {
        return new SettingsViewModel(
            _mockSettingsService.Object,
            _mockMarkdownRenderer.Object);
    }
}
