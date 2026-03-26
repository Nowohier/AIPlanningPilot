using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="MainWindowViewModel"/>.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class MainWindowViewModelTests
{
    private Mock<INavigationService> mockNavigationService = null!;
    private Mock<IFileSystemService> mockFileSystemService = null!;
    private Mock<IMarkdownRenderer> mockMarkdownRenderer = null!;
    private Mock<IConfigurationService> mockConfigService = null!;
    private Mock<IFileWatcherService> mockFileWatcherService = null!;
    private Mock<IFileViewerCoordinator> mockFileViewerCoordinator = null!;
    private Mock<IDialogService> mockDialogService = null!;

    [SetUp]
    public void SetUp()
    {
        mockNavigationService = new Mock<INavigationService>(MockBehavior.Strict);
        mockFileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
        mockMarkdownRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);
        mockConfigService = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockFileWatcherService = new Mock<IFileWatcherService>(MockBehavior.Strict);
        mockFileViewerCoordinator = new Mock<IFileViewerCoordinator>(MockBehavior.Strict);
        mockDialogService = new Mock<IDialogService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all strict mock expectations after each test.
    /// Event subscription mocks (navigation, renderer, file watcher) are excluded
    /// because their SetupAdd calls are only configured inside <see cref="CreateViewModel"/>.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        mockFileSystemService.VerifyAll();
        mockConfigService.VerifyAll();
        mockFileViewerCoordinator.VerifyAll();
        mockDialogService.VerifyAll();
    }

    private DashboardViewModel CreateDashboardViewModel()
    {
        var stateParser = new Mock<IStateParser>(MockBehavior.Strict);
        var handoverParser = new Mock<IHandoverParser>(MockBehavior.Strict);
        var navigationService = new Mock<INavigationService>(MockBehavior.Strict);
        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);

        return new DashboardViewModel(
            configService.Object,
            stateParser.Object,
            handoverParser.Object,
            fileSystemService.Object,
            navigationService.Object);
    }

    private DecisionTrackerViewModel CreateDecisionTrackerViewModel()
    {
        var decisionParser = new Mock<IDecisionParser>(MockBehavior.Strict);
        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
        var markdownRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);

        return new DecisionTrackerViewModel(
            configService.Object, decisionParser.Object,
            fileSystemService.Object, markdownRenderer.Object);
    }

    private HandoverViewModel CreateHandoverViewModel()
    {
        var handoverParser = new Mock<IHandoverParser>(MockBehavior.Strict);
        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);

        return new HandoverViewModel(
            configService.Object, handoverParser.Object, fileSystemService.Object);
    }

    private ActionHistoryViewModel CreateActionHistoryViewModel()
    {
        var actionHistoryParser = new Mock<IActionHistoryParser>(MockBehavior.Strict);
        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);

        return new ActionHistoryViewModel(
            configService.Object, actionHistoryParser.Object, fileSystemService.Object);
    }

    private SearchViewModel CreateSearchViewModel()
    {
        var searchService = new Mock<ISearchService>(MockBehavior.Strict);
        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        var navigationService = new Mock<INavigationService>(MockBehavior.Strict);

        return new SearchViewModel(
            searchService.Object, configService.Object, navigationService.Object);
    }

    private MigrationTrackerViewModel CreateMigrationTrackerViewModel()
    {
        var migrationParser = new Mock<IMigrationParser>(MockBehavior.Strict);
        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);

        return new MigrationTrackerViewModel(
            configService.Object, migrationParser.Object, fileSystemService.Object);
    }

    private TreeViewViewModel CreateTreeViewViewModel()
    {
        return new TreeViewViewModel(
            new Mock<IFileSystemService>(MockBehavior.Strict).Object,
            new Mock<IConfigurationService>(MockBehavior.Strict).Object,
            new Mock<INavigationService>(MockBehavior.Strict).Object);
    }

    private MarkdownViewerViewModel CreateMarkdownViewerViewModel()
    {
        // Uses shared mocks because MainWindowViewModel delegates file loading to MarkdownViewer
        return new MarkdownViewerViewModel(
            mockFileSystemService.Object,
            mockMarkdownRenderer.Object);
    }

    private CodeViewerViewModel CreateCodeViewerViewModel()
    {
        // Uses shared mock because MainWindowViewModel delegates file loading to CodeViewer
        return new CodeViewerViewModel(
            mockFileSystemService.Object);
    }

    private MainWindowViewModel CreateViewModel()
    {
        // MainWindowViewModel subscribes to events in its constructor
        mockNavigationService.SetupAdd(n => n.FileSelected += It.IsAny<EventHandler<string>>());
        mockMarkdownRenderer.SetupAdd(r => r.ThemeChanged += It.IsAny<Action>());
        mockFileWatcherService.SetupAdd(w => w.FileChanged += It.IsAny<Action<string>>());
        mockConfigService.Setup(c => c.ProjectName).Returns("testproject");

        return new MainWindowViewModel(
            mockNavigationService.Object,
            mockFileSystemService.Object,
            mockMarkdownRenderer.Object,
            mockConfigService.Object,
            CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(),
            CreateHandoverViewModel(),
            CreateActionHistoryViewModel(),
            CreateSearchViewModel(),
            CreateMigrationTrackerViewModel(),
            mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object,
            mockDialogService.Object,
            CreateTreeViewViewModel(),
            CreateMarkdownViewerViewModel(),
            CreateCodeViewerViewModel());
    }

    [Test]
    public void Constructor_WhenCreated_ShouldHaveDefaultTitle()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.Title.Should().Be("AI Planning Pilot - testproject");
    }

    [Test]
    public void NavigateToFile_WhenMarkdownFile_ShouldActivateMarkdownViewer()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        var filePath = $@"{rootPath}\main\STATE.md";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);

        var viewModel = CreateViewModel();
        var markdownViewer = viewModel.MarkdownViewer;
        mockFileViewerCoordinator.Setup(c => c.OpenFile(filePath)).Returns(markdownViewer);

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeSameAs(markdownViewer);
        viewModel.Title.Should().Contain("STATE.md");
    }

    [Test]
    public void NavigateToFile_WhenCodeFile_ShouldActivateCodeViewer()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        var filePath = $@"{rootPath}\scripts\sync-claude.mjs";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);

        var viewModel = CreateViewModel();
        var codeViewer = viewModel.CodeViewer;
        mockFileViewerCoordinator.Setup(c => c.OpenFile(filePath)).Returns(codeViewer);

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeSameAs(codeViewer);
    }

    [Test]
    public void NavigateToFile_WhenFileDoesNotExist_ShouldNotChangeActiveContent()
    {
        // Arrange
        var filePath = @"C:\nonexistent\file.md";
        mockFileViewerCoordinator.Setup(c => c.OpenFile(filePath)).Returns((ObservableObject?)null);

        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeNull();
    }

    [Test]
    public void NavigateToFile_WhenEmptyPath_ShouldNotChangeActiveContent()
    {
        // Arrange
        mockFileViewerCoordinator.Setup(c => c.OpenFile(string.Empty)).Returns((ObservableObject?)null);

        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToFile(string.Empty);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeNull();
    }

    [Test]
    public void NavigateToDashboard_WhenCalled_ShouldSetSelectedViewToDashboard()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToDashboard();

        // Assert
        viewModel.SelectedView.Should().Be(ActiveView.Dashboard);
    }

    [Test]
    public void NavigateToDecisions_WhenCalled_ShouldSetSelectedViewToDecisions()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToDecisions();

        // Assert
        viewModel.SelectedView.Should().Be(ActiveView.Decisions);
    }

    [Test]
    public void NavigateToHandover_WhenCalled_ShouldSetSelectedViewToHandover()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToHandover();

        // Assert
        viewModel.SelectedView.Should().Be(ActiveView.Handover);
    }

    [Test]
    public void NavigateToMigration_WhenCalled_ShouldSetSelectedViewToMigration()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToMigration();

        // Assert
        viewModel.SelectedView.Should().Be(ActiveView.Migration);
    }

    [Test]
    public void Constructor_WhenNullNavigationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            null!, mockFileSystemService.Object, mockMarkdownRenderer.Object,
            mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("navigationService");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, null!, mockMarkdownRenderer.Object,
            mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void Constructor_WhenNullMarkdownRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object, null!,
            mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("markdownRenderer");
    }

    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, null!, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullDashboardViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, null!,
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dashboardViewModel");
    }

    [Test]
    public void Constructor_WhenNullDecisionTrackerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            null!, CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("decisionTrackerViewModel");
    }

    [Test]
    public void Constructor_WhenNullHandoverViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), null!, CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("handoverViewModel");
    }

    [Test]
    public void Constructor_WhenNullActionHistoryViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), null!,
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("actionHistoryViewModel");
    }

    [Test]
    public void Constructor_WhenNullSearchViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            null!, CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("searchViewModel");
    }

    [Test]
    public void Constructor_WhenNullMigrationTrackerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), null!, mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("migrationTrackerViewModel");
    }

    [Test]
    public void Constructor_WhenNullFileWatcherService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), null!,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileWatcherService");
    }

    [Test]
    public void Constructor_WhenNullFileViewerCoordinator_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            null!, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileViewerCoordinator");
    }

    [Test]
    public void Constructor_WhenNullDialogService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, null!,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dialogService");
    }

    [Test]
    public void Constructor_WhenNullTreeViewViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            null!, CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("treeViewViewModel");
    }

    [Test]
    public void Constructor_WhenNullMarkdownViewerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), null!, CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("markdownViewerViewModel");
    }

    [Test]
    public void Constructor_WhenNullCodeViewerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            mockNavigationService.Object, mockFileSystemService.Object,
            mockMarkdownRenderer.Object, mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object, mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("codeViewerViewModel");
    }

    [Test]
    public void Initialize_WhenCalled_ShouldLoadTreeAndChildViewModels()
    {
        // Arrange
        var rootPath = @"C:\restructuring";

        var configService = new Mock<IConfigurationService>(MockBehavior.Strict);
        configService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        configService.Setup(c => c.ProjectName).Returns("testproject");

        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
        fileSystemService.Setup(fs => fs.GetDirectoryTree(rootPath, true)).Returns(new List<FileTreeNode>());
        fileSystemService.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
        fileSystemService.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var dashboard = new DashboardViewModel(
            configService.Object,
            new Mock<IStateParser>(MockBehavior.Strict).Object,
            new Mock<IHandoverParser>(MockBehavior.Strict).Object,
            fileSystemService.Object,
            new Mock<INavigationService>(MockBehavior.Strict).Object);

        var decisionTracker = new DecisionTrackerViewModel(
            configService.Object,
            new Mock<IDecisionParser>(MockBehavior.Strict).Object,
            fileSystemService.Object,
            new Mock<IMarkdownRenderer>(MockBehavior.Strict).Object);

        var handover = new HandoverViewModel(
            configService.Object,
            new Mock<IHandoverParser>(MockBehavior.Strict).Object,
            fileSystemService.Object);

        var actionHistory = new ActionHistoryViewModel(
            configService.Object,
            new Mock<IActionHistoryParser>(MockBehavior.Strict).Object,
            fileSystemService.Object);

        var search = new SearchViewModel(
            new Mock<ISearchService>(MockBehavior.Strict).Object,
            configService.Object,
            new Mock<INavigationService>(MockBehavior.Strict).Object);

        var migration = new MigrationTrackerViewModel(
            configService.Object,
            new Mock<IMigrationParser>(MockBehavior.Strict).Object,
            fileSystemService.Object);

        var treeView = new TreeViewViewModel(
            fileSystemService.Object,
            configService.Object,
            new Mock<INavigationService>(MockBehavior.Strict).Object);

        var markdownViewer = new MarkdownViewerViewModel(
            fileSystemService.Object,
            new Mock<IMarkdownRenderer>(MockBehavior.Strict).Object);

        var codeViewer = new CodeViewerViewModel(fileSystemService.Object);

        mockNavigationService.SetupAdd(n => n.FileSelected += It.IsAny<EventHandler<string>>());
        mockMarkdownRenderer.SetupAdd(r => r.ThemeChanged += It.IsAny<Action>());
        mockFileWatcherService.SetupAdd(w => w.FileChanged += It.IsAny<Action<string>>());
        mockFileWatcherService.Setup(w => w.Start(rootPath));

        var viewModel = new MainWindowViewModel(
            mockNavigationService.Object,
            fileSystemService.Object,
            mockMarkdownRenderer.Object,
            configService.Object,
            dashboard,
            decisionTracker,
            handover,
            actionHistory,
            search,
            migration,
            mockFileWatcherService.Object,
            mockFileViewerCoordinator.Object,
            mockDialogService.Object,
            treeView,
            markdownViewer,
            codeViewer);

        // Act
        viewModel.Initialize();

        // Assert
        dashboard.IsLoaded.Should().BeTrue();
        decisionTracker.IsLoaded.Should().BeTrue();
        handover.IsLoaded.Should().BeTrue();
        viewModel.LastRefreshedTime.Should().NotBeEmpty();
        mockFileWatcherService.Verify(w => w.Start(rootPath), Times.Once);
    }

    [Test]
    public void OpenSettings_WhenCalled_ShouldCallDialogService()
    {
        // Arrange
        mockDialogService.Setup(d => d.ShowSettingsDialog());
        var viewModel = CreateViewModel();

        // Act
        viewModel.OpenSettings();

        // Assert
        mockDialogService.Verify(d => d.ShowSettingsDialog(), Times.Once);
    }

    [Test]
    public void Dispose_WhenCalled_ShouldUnsubscribeFromEvents()
    {
        // Arrange
        mockNavigationService.SetupRemove(n => n.FileSelected -= It.IsAny<EventHandler<string>>());
        mockMarkdownRenderer.SetupRemove(r => r.ThemeChanged -= It.IsAny<Action>());
        mockFileWatcherService.SetupRemove(w => w.FileChanged -= It.IsAny<Action<string>>());

        var viewModel = CreateViewModel();

        // Act
        var act = () => viewModel.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
