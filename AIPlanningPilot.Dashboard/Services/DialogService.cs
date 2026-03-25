using System.Windows;
using AIPlanningPilot.Dashboard.ViewModels;
using AIPlanningPilot.Dashboard.Views;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IDialogService"/> using WPF window creation.
/// </summary>
public class DialogService : IDialogService
{
    private readonly Func<SettingsViewModel> settingsViewModelFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogService"/> class.
    /// </summary>
    /// <param name="settingsViewModelFactory">Factory for creating SettingsViewModel instances.</param>
    public DialogService(Func<SettingsViewModel> settingsViewModelFactory)
    {
        this.settingsViewModelFactory = settingsViewModelFactory ?? throw new ArgumentNullException(nameof(settingsViewModelFactory));
    }

    /// <inheritdoc />
    public void ShowSettingsDialog()
    {
        var settingsVm = settingsViewModelFactory();
        var settingsWindow = new SettingsWindow
        {
            DataContext = settingsVm,
            Owner = Application.Current.MainWindow
        };
        settingsWindow.ShowDialog();
    }
}
