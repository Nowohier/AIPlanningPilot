using Microsoft.Extensions.DependencyInjection;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard;

/// <summary>
/// Configures the dependency injection container for the Restructuring Dashboard application.
/// Registers all services, parsers, and view models.
/// </summary>
internal static class AppStartup
{
    /// <summary>
    /// Registers all application services, parsers, and view models into the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="restructuringRootPath">The absolute path to the restructuring directory.</param>
    public static void ConfigureServices(IServiceCollection services, string restructuringRootPath)
    {
        // Configuration
        services.AddSingleton<IConfigurationService>(new ConfigurationService(restructuringRootPath));

        // Settings
        services.AddSingleton<ISettingsService, SettingsService>();

        // Core services
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IMarkdownRenderer, MarkdownRendererService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<ISearchService, SearchService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IDocxRenderer, DocxRendererService>();
        services.AddSingleton<IDrawioRenderer, DrawioRendererService>();
        services.AddSingleton<Func<SettingsViewModel>>(sp => () => sp.GetRequiredService<SettingsViewModel>());
        services.AddSingleton<IDialogService, DialogService>();

        // Parsers
        services.AddTransient<IStateParser, StateParser>();
        services.AddTransient<IDecisionParser, DecisionParser>();
        services.AddTransient<IHandoverParser, HandoverParser>();
        services.AddTransient<IActionHistoryParser, ActionHistoryParser>();
        services.AddTransient<IMigrationParser, MigrationParser>();

        // ViewModels - Singleton to match actual lifetime (captured by MainWindowViewModel)
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<TreeViewViewModel>();
        services.AddSingleton<MarkdownViewerViewModel>();
        services.AddSingleton<CodeViewerViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<DecisionTrackerViewModel>();
        services.AddSingleton<HandoverViewModel>();
        services.AddSingleton<ActionHistoryViewModel>();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<MigrationTrackerViewModel>();
        services.AddTransient<SettingsViewModel>();
    }
}
