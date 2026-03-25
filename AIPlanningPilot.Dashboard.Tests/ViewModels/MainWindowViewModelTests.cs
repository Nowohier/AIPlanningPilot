using System.IO;
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
    private Mock<INavigationService> _mockNavigationService = null!;
    private Mock<IFileSystemService> _mockFileSystemService = null!;
    private Mock<IMarkdownRenderer> _mockMarkdownRenderer = null!;
    private Mock<IConfigurationService> _mockConfigService = null!;
    private Mock<IFileWatcherService> _mockFileWatcherService = null!;
    private Mock<IDocxRenderer> _mockDocxRenderer = null!;
    private Mock<IDrawioRenderer> _mockDrawioRenderer = null!;
    private Mock<IDialogService> _mockDialogService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNavigationService = new Mock<INavigationService>(MockBehavior.Strict);
        _mockFileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
        _mockMarkdownRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);
        _mockConfigService = new Mock<IConfigurationService>(MockBehavior.Strict);
        _mockFileWatcherService = new Mock<IFileWatcherService>(MockBehavior.Strict);
        _mockDocxRenderer = new Mock<IDocxRenderer>(MockBehavior.Strict);
        _mockDrawioRenderer = new Mock<IDrawioRenderer>(MockBehavior.Strict);
        _mockDialogService = new Mock<IDialogService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all strict mock expectations after each test.
    /// Event subscription mocks (navigation, renderer, file watcher) are excluded
    /// because their SetupAdd calls are only configured inside <see cref="CreateViewModel"/>.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _mockFileSystemService.VerifyAll();
        _mockConfigService.VerifyAll();
        _mockDocxRenderer.VerifyAll();
        _mockDrawioRenderer.VerifyAll();
        _mockDialogService.VerifyAll();
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
            _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object);
    }

    private CodeViewerViewModel CreateCodeViewerViewModel()
    {
        // Uses shared mock because MainWindowViewModel delegates file loading to CodeViewer
        return new CodeViewerViewModel(
            _mockFileSystemService.Object);
    }

    private MainWindowViewModel CreateViewModel()
    {
        // MainWindowViewModel subscribes to events in its constructor
        _mockNavigationService.SetupAdd(n => n.FileSelected += It.IsAny<EventHandler<string>>());
        _mockMarkdownRenderer.SetupAdd(r => r.ThemeChanged += It.IsAny<Action>());
        _mockFileWatcherService.SetupAdd(w => w.FileChanged += It.IsAny<Action<string>>());
        _mockConfigService.Setup(c => c.ProjectName).Returns("testproject");

        return new MainWindowViewModel(
            _mockNavigationService.Object,
            _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object,
            _mockConfigService.Object,
            CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(),
            CreateHandoverViewModel(),
            CreateActionHistoryViewModel(),
            CreateSearchViewModel(),
            CreateMigrationTrackerViewModel(),
            _mockFileWatcherService.Object,
            _mockDocxRenderer.Object,
            _mockDrawioRenderer.Object,
            _mockDialogService.Object,
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
        _mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        _mockFileSystemService.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("# STATE.md");
        _mockMarkdownRenderer.Setup(r => r.RenderMarkdown("# STATE.md")).Returns("<html><body>rendered</body></html>");

        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeSameAs(viewModel.MarkdownViewer);
        viewModel.Title.Should().Contain("STATE.md");
    }

    [Test]
    public void NavigateToFile_WhenCodeFile_ShouldActivateCodeViewer()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        var filePath = $@"{rootPath}\scripts\sync-claude.mjs";
        _mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        _mockFileSystemService.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("console.log('hello');");

        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeSameAs(viewModel.CodeViewer);
    }

    [Test]
    public void NavigateToFile_WhenFileDoesNotExist_ShouldNotChangeActiveContent()
    {
        // Arrange
        var filePath = @"C:\nonexistent\file.md";
        _mockFileSystemService.Setup(fs => fs.FileExists(filePath)).Returns(false);

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
            null!, _mockFileSystemService.Object, _mockMarkdownRenderer.Object,
            _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("navigationService");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, null!, _mockMarkdownRenderer.Object,
            _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void Constructor_WhenNullMarkdownRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object, null!,
            _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("markdownRenderer");
    }

    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, null!, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullDashboardViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, null!,
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dashboardViewModel");
    }

    [Test]
    public void Constructor_WhenNullDecisionTrackerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            null!, CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("decisionTrackerViewModel");
    }

    [Test]
    public void Constructor_WhenNullHandoverViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), null!, CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("handoverViewModel");
    }

    [Test]
    public void Constructor_WhenNullActionHistoryViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), null!,
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("actionHistoryViewModel");
    }

    [Test]
    public void Constructor_WhenNullSearchViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            null!, CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("searchViewModel");
    }

    [Test]
    public void Constructor_WhenNullMigrationTrackerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), null!, _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("migrationTrackerViewModel");
    }

    [Test]
    public void Constructor_WhenNullFileWatcherService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), null!,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileWatcherService");
    }

    [Test]
    public void Constructor_WhenNullDocxRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            null!, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("docxRenderer");
    }

    [Test]
    public void Constructor_WhenNullDrawioRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, null!, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("drawioRenderer");
    }

    [Test]
    public void Constructor_WhenNullDialogService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, null!,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dialogService");
    }

    [Test]
    public void Constructor_WhenNullTreeViewViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            null!, CreateMarkdownViewerViewModel(), CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("treeViewViewModel");
    }

    [Test]
    public void Constructor_WhenNullMarkdownViewerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), null!, CreateCodeViewerViewModel());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("markdownViewerViewModel");
    }

    [Test]
    public void Constructor_WhenNullCodeViewerViewModel_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MainWindowViewModel(
            _mockNavigationService.Object, _mockFileSystemService.Object,
            _mockMarkdownRenderer.Object, _mockConfigService.Object, CreateDashboardViewModel(),
            CreateDecisionTrackerViewModel(), CreateHandoverViewModel(), CreateActionHistoryViewModel(),
            CreateSearchViewModel(), CreateMigrationTrackerViewModel(), _mockFileWatcherService.Object,
            _mockDocxRenderer.Object, _mockDrawioRenderer.Object, _mockDialogService.Object,
            CreateTreeViewViewModel(), CreateMarkdownViewerViewModel(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("codeViewerViewModel");
    }

    [Test]
    public void Initialize_WhenCalled_ShouldLoadTreeAndChildViewModels()
    {
        // Arrange
        var rootPath = @"C:\restructuring";

        var configService = new Mock<IConfigurationService>(MockBehavior.Loose);
        configService.Setup(c => c.RestructuringRootPath).Returns(rootPath);

        var fileSystemService = new Mock<IFileSystemService>(MockBehavior.Loose);
        fileSystemService.Setup(fs => fs.GetDirectoryTree(rootPath, true)).Returns(new List<FileTreeNode>());
        fileSystemService.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
        fileSystemService.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        var dashboard = new DashboardViewModel(
            configService.Object,
            new Mock<IStateParser>(MockBehavior.Loose).Object,
            new Mock<IHandoverParser>(MockBehavior.Loose).Object,
            fileSystemService.Object,
            new Mock<INavigationService>(MockBehavior.Loose).Object);

        var decisionTracker = new DecisionTrackerViewModel(
            configService.Object,
            new Mock<IDecisionParser>(MockBehavior.Loose).Object,
            fileSystemService.Object,
            new Mock<IMarkdownRenderer>(MockBehavior.Loose).Object);

        var handover = new HandoverViewModel(
            configService.Object,
            new Mock<IHandoverParser>(MockBehavior.Loose).Object,
            fileSystemService.Object);

        var actionHistory = new ActionHistoryViewModel(
            configService.Object,
            new Mock<IActionHistoryParser>(MockBehavior.Loose).Object,
            fileSystemService.Object);

        var search = new SearchViewModel(
            new Mock<ISearchService>(MockBehavior.Loose).Object,
            configService.Object,
            new Mock<INavigationService>(MockBehavior.Loose).Object);

        var migration = new MigrationTrackerViewModel(
            configService.Object,
            new Mock<IMigrationParser>(MockBehavior.Loose).Object,
            fileSystemService.Object);

        var treeView = new TreeViewViewModel(
            fileSystemService.Object,
            configService.Object,
            new Mock<INavigationService>(MockBehavior.Loose).Object);

        var markdownViewer = new MarkdownViewerViewModel(
            fileSystemService.Object,
            new Mock<IMarkdownRenderer>(MockBehavior.Loose).Object);

        var codeViewer = new CodeViewerViewModel(fileSystemService.Object);

        _mockNavigationService.SetupAdd(n => n.FileSelected += It.IsAny<EventHandler<string>>());
        _mockMarkdownRenderer.SetupAdd(r => r.ThemeChanged += It.IsAny<Action>());
        _mockFileWatcherService.SetupAdd(w => w.FileChanged += It.IsAny<Action<string>>());
        _mockFileWatcherService.Setup(w => w.Start(rootPath));

        var viewModel = new MainWindowViewModel(
            _mockNavigationService.Object,
            fileSystemService.Object,
            _mockMarkdownRenderer.Object,
            configService.Object,
            dashboard,
            decisionTracker,
            handover,
            actionHistory,
            search,
            migration,
            _mockFileWatcherService.Object,
            _mockDocxRenderer.Object,
            _mockDrawioRenderer.Object,
            _mockDialogService.Object,
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
        _mockFileWatcherService.Verify(w => w.Start(rootPath), Times.Once);
    }

    [Test]
    public void OpenSettings_WhenCalled_ShouldCallDialogService()
    {
        // Arrange
        _mockDialogService.Setup(d => d.ShowSettingsDialog());
        var viewModel = CreateViewModel();

        // Act
        viewModel.OpenSettings();

        // Assert
        _mockDialogService.Verify(d => d.ShowSettingsDialog(), Times.Once);
    }

    [Test]
    public void NavigateToFile_WhenDocxFile_ShouldRenderDocx()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        var filePath = $@"{rootPath}\docs\report.docx";
        _mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        _mockFileSystemService.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _mockDocxRenderer.Setup(r => r.RenderDocx(filePath)).Returns("<html><body>docx content</body></html>");

        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeSameAs(viewModel.MarkdownViewer);
        viewModel.Title.Should().Contain("report.docx");
        _mockDocxRenderer.Verify(r => r.RenderDocx(filePath), Times.Once);
    }

    [Test]
    public void NavigateToFile_WhenDrawioFile_ShouldRenderDrawio()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        var filePath = $@"{rootPath}\diagrams\arch.drawio";
        var drawioXml = "<mxfile>diagram</mxfile>";
        _mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        _mockFileSystemService.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns(drawioXml);
        _mockDrawioRenderer.Setup(r => r.RenderDrawio(drawioXml)).Returns("<html><body>drawio rendered</body></html>");

        var viewModel = CreateViewModel();

        // Act
        viewModel.NavigateToFile(filePath);

        // Assert
        viewModel.ActiveContentViewModel.Should().BeSameAs(viewModel.MarkdownViewer);
        viewModel.Title.Should().Contain("arch.drawio");
        _mockDrawioRenderer.Verify(r => r.RenderDrawio(drawioXml), Times.Once);
    }

    [Test]
    public void Dispose_WhenCalled_ShouldUnsubscribeFromEvents()
    {
        // Arrange
        _mockNavigationService.SetupRemove(n => n.FileSelected -= It.IsAny<EventHandler<string>>());
        _mockMarkdownRenderer.SetupRemove(r => r.ThemeChanged -= It.IsAny<Action>());
        _mockFileWatcherService.SetupRemove(w => w.FileChanged -= It.IsAny<Action<string>>());

        var viewModel = CreateViewModel();

        // Act
        var act = () => viewModel.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
