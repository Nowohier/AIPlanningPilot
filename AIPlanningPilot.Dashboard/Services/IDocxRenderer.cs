namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Renders Word (.docx) documents into a complete HTML document string for display in a WebView2 control.
/// </summary>
public interface IDocxRenderer
{
    /// <summary>
    /// Converts the specified .docx file into a complete HTML document string
    /// with the currently selected CSS theme applied.
    /// </summary>
    /// <param name="filePath">The absolute path to the .docx file.</param>
    /// <returns>A complete HTML document string suitable for <c>WebView2.NavigateToString()</c>.</returns>
    string RenderDocx(string filePath);
}
