using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="MarkdownRendererService"/>.
/// </summary>
[TestFixture]
public class MarkdownRendererServiceTests
{
    private MarkdownRendererService service = null!;

    /// <summary>
    /// Creates a fresh instance of the service before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        service = new MarkdownRendererService();
    }

    /// <summary>
    /// Verifies that rendered HTML contains the original markdown content.
    /// </summary>
    [Test]
    public void RenderMarkdown_WhenCalledWithContent_ShouldProduceHtmlContainingMarkdownContent()
    {
        // Arrange
        var markdown = "# Hello World";

        // Act
        var result = service.RenderMarkdown(markdown);

        // Assert
        result.Should().Contain("Hello World");
    }

    /// <summary>
    /// Verifies that rendered HTML is a full document with a DOCTYPE declaration.
    /// </summary>
    [Test]
    public void RenderMarkdown_WhenCalled_ShouldContainDoctypeHtml()
    {
        // Arrange
        var markdown = "Some text";

        // Act
        var result = service.RenderMarkdown(markdown);

        // Assert
        result.Should().Contain("<!DOCTYPE html>");
    }

    /// <summary>
    /// Verifies that an HTML fragment is wrapped in a full HTML document.
    /// </summary>
    [Test]
    public void WrapHtmlFragment_WhenCalledWithFragment_ShouldWrapInFullHtml()
    {
        // Arrange
        var fragment = "<p>Test paragraph</p>";

        // Act
        var result = service.WrapHtmlFragment(fragment);

        // Assert
        result.Should().Contain("<!DOCTYPE html>");
        result.Should().Contain("<html>");
        result.Should().Contain("</html>");
        result.Should().Contain(fragment);
    }

    /// <summary>
    /// Verifies that exactly five themes are available.
    /// </summary>
    [Test]
    public void AvailableThemes_WhenAccessed_ShouldContainFiveThemes()
    {
        // Arrange & Act
        var themes = service.AvailableThemes;

        // Assert
        themes.Should().HaveCount(5);
    }

    /// <summary>
    /// Verifies that "GitHub Light" is among the available themes.
    /// </summary>
    [Test]
    public void AvailableThemes_WhenAccessed_ShouldContainGitHubLight()
    {
        // Arrange & Act
        var themes = service.AvailableThemes;

        // Assert
        themes.Should().Contain("GitHub Light");
    }

    /// <summary>
    /// Verifies that the default selected theme is "GitHub Light".
    /// </summary>
    [Test]
    public void SelectedThemeName_WhenNotSet_ShouldDefaultToGitHubLight()
    {
        // Arrange & Act
        var themeName = service.SelectedThemeName;

        // Assert
        themeName.Should().Be("GitHub Light");
    }

    /// <summary>
    /// Verifies that setting a valid theme updates the selected theme.
    /// </summary>
    [Test]
    public void SelectedThemeName_WhenSetToValidTheme_ShouldUpdate()
    {
        // Arrange
        var newTheme = "GitHub Dark";

        // Act
        service.SelectedThemeName = newTheme;

        // Assert
        service.SelectedThemeName.Should().Be(newTheme);
    }

    /// <summary>
    /// Verifies that setting an invalid theme does not change the selection.
    /// </summary>
    [Test]
    public void SelectedThemeName_WhenSetToInvalidTheme_ShouldNotChange()
    {
        // Arrange
        var originalTheme = service.SelectedThemeName;

        // Act
        service.SelectedThemeName = "NonExistent Theme";

        // Assert
        service.SelectedThemeName.Should().Be(originalTheme);
    }

    /// <summary>
    /// Verifies that the ThemeChanged event fires when the theme is changed.
    /// </summary>
    [Test]
    public void ThemeChanged_WhenThemeChanges_ShouldFireEvent()
    {
        // Arrange
        var eventFired = false;
        service.ThemeChanged += () => eventFired = true;

        // Act
        service.SelectedThemeName = "GitHub Dark";

        // Assert
        eventFired.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the ThemeChanged event does not fire when the same theme is set.
    /// </summary>
    [Test]
    public void ThemeChanged_WhenSetToSameTheme_ShouldNotFireEvent()
    {
        // Arrange
        var eventFired = false;
        service.ThemeChanged += () => eventFired = true;

        // Act
        service.SelectedThemeName = service.SelectedThemeName;

        // Assert
        eventFired.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that AssetsDirectory returns a non-empty path.
    /// </summary>
    [Test]
    public void AssetsDirectory_WhenAccessed_ShouldReturnNonEmptyPath()
    {
        // Arrange & Act
        var path = service.AssetsDirectory;

        // Assert
        path.Should().NotBeNullOrEmpty();
    }
}
