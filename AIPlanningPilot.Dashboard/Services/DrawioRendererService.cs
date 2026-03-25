using System.Text.Json;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IDrawioRenderer"/> using the draw.io
/// viewer-static.min.js to render diagrams in a WebView2 control.
/// The viewer JS is served from the virtual host set up by the WebView2 views.
/// </summary>
public class DrawioRendererService : IDrawioRenderer
{
    /// <inheritdoc />
    public string RenderDrawio(string drawioXml)
    {
        // Escape the XML for safe embedding in a JSON string attribute
        var escapedXml = JsonSerializer.Serialize(drawioXml);
        // Remove the surrounding quotes added by JsonSerializer
        escapedXml = escapedXml[1..^1];

        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="color-scheme" content="light">
                <style>
                    html, body {
                        margin: 0;
                        padding: 0;
                        width: 100%;
                        height: 100%;
                        overflow: hidden;
                        background: #fff;
                    }
                    .mxgraph {
                        width: 100% !important;
                        height: 100% !important;
                    }
                    .geDiagramContainer {
                        width: 100% !important;
                        height: 100% !important;
                    }
                </style>
            </head>
            <body>
                <div class="mxgraph" data-mxgraph='{"highlight":"{{WebViewConstants.DrawioHighlightColor}}","nav":true,"resize":true,"toolbar":"zoom layers","edit":false,"xml":"{{escapedXml}}"}'></div>
                <script src="{{WebViewConstants.AssetBaseUrl}}viewer-static.min.js"></script>
                <script>
                    window.addEventListener('resize', function() {
                        var graphs = document.querySelectorAll('.mxgraph');
                        graphs.forEach(function(el) {
                            if (el.mxGraphModel) {
                                el.mxGraphModel.fit();
                            }
                        });
                        // Force re-layout of the viewer container
                        var containers = document.querySelectorAll('.geDiagramContainer');
                        containers.forEach(function(c) {
                            c.style.width = '100%';
                            c.style.height = '100vh';
                        });
                    });
                </script>
            </body>
            </html>
            """;
    }
}
