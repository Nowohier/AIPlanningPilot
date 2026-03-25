namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Shared constants for WebView2 virtual host configuration used by renderers and views.
/// </summary>
internal static class WebViewConstants
{
    /// <summary>
    /// The virtual host name used for serving local assets via WebView2.
    /// Must match the host name configured in SetVirtualHostNameToFolderMapping.
    /// </summary>
    public const string VirtualHostName = "app.assets";

    /// <summary>
    /// The base URL prefix for referencing virtual-hosted assets in HTML content.
    /// </summary>
    public const string AssetBaseUrl = "https://app.assets/";

    /// <summary>
    /// The highlight color for draw.io diagrams.
    /// </summary>
    public const string DrawioHighlightColor = "#0066CC";

    /// <summary>
    /// Empty HTML document used as fallback when no content is available.
    /// </summary>
    public const string EmptyHtml = "<html><body></body></html>";
}
