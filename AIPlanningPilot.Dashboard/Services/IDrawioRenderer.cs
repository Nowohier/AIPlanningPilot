namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Renders draw.io (.drawio) diagram files into a complete HTML document string
/// for display in a WebView2 control using the draw.io viewer.
/// </summary>
public interface IDrawioRenderer
{
    /// <summary>
    /// Converts the specified .drawio XML content into a complete HTML document string
    /// with the embedded draw.io viewer.
    /// </summary>
    /// <param name="drawioXml">The raw XML content of the .drawio file.</param>
    /// <returns>A complete HTML document string suitable for <c>WebView2.NavigateToString()</c>.</returns>
    string RenderDrawio(string drawioXml);
}
