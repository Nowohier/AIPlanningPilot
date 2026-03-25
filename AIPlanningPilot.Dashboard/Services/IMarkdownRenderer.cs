namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Renders markdown text into a full HTML document string for display in a WebView2 control.
/// Supports switching between available CSS themes at runtime.
/// </summary>
public interface IMarkdownRenderer
{
    /// <summary>
    /// Gets the directory path where WebView2 assets are extracted.
    /// </summary>
    string AssetsDirectory { get; }

    /// <summary>
    /// Gets the list of available theme names for the markdown renderer.
    /// </summary>
    string[] AvailableThemes { get; }

    /// <summary>
    /// Gets or sets the name of the currently selected markdown theme.
    /// </summary>
    string SelectedThemeName { get; set; }

    /// <summary>
    /// Occurs when the selected theme changes.
    /// </summary>
    event Action? ThemeChanged;

    /// <summary>
    /// Converts the specified markdown text into a complete HTML document string
    /// with the currently selected CSS theme applied.
    /// </summary>
    /// <param name="markdownText">The raw markdown text to render.</param>
    /// <returns>A complete HTML document string suitable for <c>WebView2.NavigateToString()</c>.</returns>
    string RenderMarkdown(string markdownText);

    /// <summary>
    /// Wraps an HTML fragment in a complete HTML document with the currently selected CSS theme.
    /// Used for non-markdown content (e.g., .docx) that produces raw HTML fragments.
    /// </summary>
    /// <param name="htmlFragment">The HTML fragment to wrap.</param>
    /// <returns>A complete HTML document string suitable for <c>WebView2.NavigateToString()</c>.</returns>
    string WrapHtmlFragment(string htmlFragment);
}
