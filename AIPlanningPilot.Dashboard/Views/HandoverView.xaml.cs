using System.Windows;
using System.Windows.Controls;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the handover notes view.
/// </summary>
public partial class HandoverView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandoverView"/> class.
    /// </summary>
    public HandoverView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Loaded event to trigger data loading.
    /// </summary>
    private void View_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is HandoverViewModel vm && !vm.IsLoaded)
        {
            vm.LoadDataCommand.Execute(null);
        }
    }
}
