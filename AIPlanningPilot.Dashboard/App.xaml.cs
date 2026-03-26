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
    /// Handles the application startup event, configures DI, and shows the main window.
    /// </summary>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var restructuringRoot = RootPathResolver.Resolve(e.Args, AppDomain.CurrentDomain.BaseDirectory);

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
}
