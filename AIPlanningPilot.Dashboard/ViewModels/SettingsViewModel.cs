using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the settings window. Allows the user to configure
/// application preferences such as the markdown rendering theme.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService settingsService;
    private readonly IMarkdownRenderer markdownRenderer;

    /// <summary>
    /// Gets or sets the selected markdown theme name.
    /// </summary>
    [ObservableProperty]
    private string selectedThemeName = string.Empty;

    /// <summary>
    /// Gets the list of available markdown theme names.
    /// </summary>
    public string[] AvailableThemes => markdownRenderer.AvailableThemes;

    /// <summary>
    /// Raised when the settings window should close.
    /// The boolean parameter indicates whether the user saved (true) or cancelled (false).
    /// </summary>
    public event Action<bool>? CloseRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settingsService">Service for persisting settings.</param>
    /// <param name="markdownRenderer">Service for markdown rendering and theme management.</param>
    public SettingsViewModel(ISettingsService settingsService, IMarkdownRenderer markdownRenderer)
    {
        this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        this.markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));

        SelectedThemeName = this.markdownRenderer.SelectedThemeName;
    }

    /// <summary>
    /// Applies the selected theme and saves settings to disk.
    /// </summary>
    [RelayCommand]
    public void Save()
    {
        markdownRenderer.SelectedThemeName = SelectedThemeName;
        settingsService.SelectedThemeName = SelectedThemeName;
        settingsService.Save();
        CloseRequested?.Invoke(true);
    }

    /// <summary>
    /// Cancels the settings dialog without saving changes.
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        CloseRequested?.Invoke(false);
    }
}
