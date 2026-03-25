using System.Windows;
using System.Windows.Controls;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the dashboard view control.
/// Triggers data loading when the view is first loaded.
/// </summary>
public partial class DashboardView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardView"/> class.
    /// </summary>
    public DashboardView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Loaded event to trigger data loading.
    /// </summary>
    private void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel && !viewModel.IsLoaded)
        {
            viewModel.LoadDataCommand.Execute(null);
        }
    }
}
