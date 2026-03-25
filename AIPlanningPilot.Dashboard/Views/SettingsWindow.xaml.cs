using AIPlanningPilot.Dashboard.ViewModels;
using MahApps.Metro.Controls;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the settings window. Subscribes to the ViewModel's close event.
/// </summary>
public partial class SettingsWindow : MetroWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsWindow"/> class.
    /// </summary>
    public SettingsWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Subscribes to the ViewModel's <see cref="SettingsViewModel.CloseRequested"/> event
    /// when the DataContext is set.
    /// </summary>
    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SettingsViewModel oldVm)
        {
            oldVm.CloseRequested -= OnCloseRequested;
        }

        if (e.NewValue is SettingsViewModel newVm)
        {
            newVm.CloseRequested += OnCloseRequested;
        }
    }

    /// <summary>
    /// Handles the close request from the ViewModel.
    /// </summary>
    /// <param name="saved">True if the user saved, false if cancelled.</param>
    private void OnCloseRequested(bool saved)
    {
        DialogResult = saved;
        Close();
    }
}
