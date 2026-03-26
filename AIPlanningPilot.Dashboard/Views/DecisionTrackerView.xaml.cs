using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the decision tracker view.
/// Manages WebView2 initialization and content updates for the decision detail panel.
/// </summary>
public partial class DecisionTrackerView : UserControl
{
    private bool isWebViewReady;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecisionTrackerView"/> class.
    /// </summary>
    public DecisionTrackerView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Initializes the WebView2 control and triggers data loading when the view is first loaded.
    /// </summary>
    private async void View_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DecisionTrackerViewModel vm && !vm.IsLoaded)
        {
            vm.LoadDataCommand.Execute(null);
        }

        if (isWebViewReady)
        {
            return;
        }

        if (DataContext is not DecisionTrackerViewModel viewModel)
        {
            return;
        }

        await WebViewHelper.InitializeAsync(DecisionWebView, viewModel.AssetsDirectory);
        isWebViewReady = true;

        UpdateWebViewContent();
    }

    /// <summary>
    /// Manages property change subscriptions when the DataContext changes.
    /// </summary>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyPropertyChanged oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is INotifyPropertyChanged newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }

        UpdateWebViewContent();
    }

    /// <summary>
    /// Updates the WebView2 content when the SelectedDecisionHtml property changes.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DecisionTrackerViewModel.SelectedDecisionHtml))
        {
            UpdateWebViewContent();
        }
    }

    /// <summary>
    /// Pushes the current ViewModel's HTML content to the WebView2 control.
    /// </summary>
    private void UpdateWebViewContent()
    {
        if (!isWebViewReady || DataContext is not DecisionTrackerViewModel vm)
        {
            return;
        }

        WebViewHelper.NavigateToHtml(DecisionWebView, vm.SelectedDecisionHtml);
    }
}
