using System.ComponentModel;
using System.Diagnostics;
using AIPlanningPilot.Dashboard.Services;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Shared helper for initializing and managing WebView2 controls across views.
/// Handles CoreWebView2 initialization, virtual host mapping, and content updates.
/// </summary>
internal static class WebViewHelper
{
    /// <summary>
    /// Initializes the WebView2 control with virtual host mapping for local assets.
    /// </summary>
    /// <param name="webView">The WebView2 control to initialize.</param>
    /// <param name="assetsDirectory">The local directory to map as a virtual host.</param>
    /// <returns>A task that completes when the WebView2 is ready.</returns>
    public static async Task InitializeAsync(WebView2 webView, string assetsDirectory)
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                WebViewConstants.VirtualHostName,
                assetsDirectory,
                CoreWebView2HostResourceAccessKind.Allow);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WebView2 initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates the WebView2 to the specified HTML content, or to empty HTML if null.
    /// </summary>
    /// <param name="webView">The WebView2 control.</param>
    /// <param name="html">The HTML content, or null for empty content.</param>
    public static void NavigateToHtml(WebView2 webView, string? html)
    {
        webView.NavigateToString(html ?? WebViewConstants.EmptyHtml);
    }
}
