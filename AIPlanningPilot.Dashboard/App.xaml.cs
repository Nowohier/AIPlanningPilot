using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.Views;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard;

/// <summary>
/// Application entry point. Configures the dependency injection container
/// and launches the main window.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    /// <summary>
    /// Gets the application-wide service provider for dependency resolution.
    /// </summary>
    public ServiceProvider ServiceProvider => serviceProvider
        ?? throw new InvalidOperationException("ServiceProvider has not been initialized.");

    /// <summary>
    /// Handles the application startup event, configures DI, and shows the main window.
    /// </summary>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var restructuringRoot = ResolveRestructuringRoot(e.Args);

        var services = new ServiceCollection();
        AppStartup.ConfigureServices(services, restructuringRoot);
        serviceProvider = services.BuildServiceProvider();

        // Load persisted settings and apply to renderer
        var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        settingsService.Load();
        var markdownRenderer = serviceProvider.GetRequiredService<IMarkdownRenderer>();
        markdownRenderer.SelectedThemeName = settingsService.SelectedThemeName;

        var mainWindow = new MainWindow
        {
            DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>()
        };

        MainWindow = mainWindow;
        mainWindow.Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        serviceProvider?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Resolves the restructuring root directory from command-line arguments
    /// or falls back to walking up the directory tree to find a known marker file.
    /// </summary>
    /// <param name="args">Command-line arguments. The first argument, if provided, is used as the root path.</param>
    /// <returns>The absolute path to the restructuring directory.</returns>
    private static string ResolveRestructuringRoot(string[] args)
    {
        if (args.Length > 0 && Directory.Exists(args[0]))
        {
            return Path.GetFullPath(args[0]);
        }

        // Walk up from the executable directory looking for a sibling containing main/STATE.md
        var current = AppDomain.CurrentDomain.BaseDirectory;
        for (var i = 0; i < 10; i++)
        {
            var parent = Path.GetDirectoryName(current);
            if (parent is null || parent == current)
            {
                break;
            }

            current = parent;

            if (Directory.Exists(current))
            {
                foreach (var dir in Directory.GetDirectories(current))
                {
                    if (File.Exists(Path.Combine(dir, "main", "STATE.md")))
                    {
                        return dir;
                    }
                }
            }
        }

        // Fallback: current directory
        return Directory.GetCurrentDirectory();
    }
}
