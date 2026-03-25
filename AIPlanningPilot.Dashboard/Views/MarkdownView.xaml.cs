using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the markdown viewer control.
/// Displays rendered markdown content in a WebView2 control.
/// </summary>
public partial class MarkdownView : UserControl
{
    private bool isWebViewReady;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownView"/> class.
    /// </summary>
    public MarkdownView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Initializes the WebView2 control when the view is first loaded.
    /// </summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (isWebViewReady)
        {
            return;
        }

        var assetsDir = ((App)Application.Current).ServiceProvider.GetRequiredService<IMarkdownRenderer>().AssetsDirectory;
        await WebViewHelper.InitializeAsync(MarkdownWebView, assetsDir);
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
    /// Updates the WebView2 content when the RenderedHtml property changes.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MarkdownViewerViewModel.RenderedHtml))
        {
            UpdateWebViewContent();
        }
    }

    /// <summary>
    /// Pushes the current ViewModel's HTML content to the WebView2 control.
    /// </summary>
    private void UpdateWebViewContent()
    {
        if (!isWebViewReady || DataContext is not MarkdownViewerViewModel vm)
        {
            return;
        }

        WebViewHelper.NavigateToHtml(MarkdownWebView, vm.RenderedHtml);
    }
}
