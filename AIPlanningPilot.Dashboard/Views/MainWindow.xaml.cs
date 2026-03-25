using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the main application window.
/// Handles initialization, keyboard shortcuts, and search field focus events.
/// </summary>
public partial class MainWindow : MetroWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += MainWindow_KeyDown;
    }

    /// <summary>
    /// Handles the Loaded event to initialize the ViewModel.
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.InitializeCommand.Execute(null);
        }
    }

    /// <summary>
    /// Handles keyboard shortcuts for the application.
    /// </summary>
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }

        if (e.Key == Key.Escape && viewModel.Search.IsOverlayVisible)
        {
            viewModel.Search.HideOverlay();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles the search text box gaining focus. Re-shows results overlay
    /// if a previous search exists.
    /// </summary>
    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Search.OnSearchFieldGotFocus();
        }
    }

    /// <summary>
    /// Handles click on the search container border (preview/tunnel event).
    /// Fires reliably even when the TextBox already has focus, because the
    /// TextBox handles MouseLeftButtonDown and prevents bubbling.
    /// </summary>
    private void SearchBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Search.OnSearchFieldGotFocus();
        }
    }
}
