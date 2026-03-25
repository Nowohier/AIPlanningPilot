using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="DashboardViewModel"/>.
/// </summary>
[TestFixture]
public class DashboardViewModelTests
{
    private Mock<IConfigurationService> _mockConfig = null!;
    private Mock<IStateParser> _mockStateParser = null!;
    private Mock<IHandoverParser> _mockHandoverParser = null!;
    private Mock<IFileSystemService> _mockFileSystem = null!;
    private Mock<INavigationService> _mockNavigation = null!;

    /// <summary>
    /// Initializes mock dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        _mockStateParser = new Mock<IStateParser>(MockBehavior.Strict);
        _mockHandoverParser = new Mock<IHandoverParser>(MockBehavior.Strict);
        _mockFileSystem = new Mock<IFileSystemService>(MockBehavior.Strict);
        _mockNavigation = new Mock<INavigationService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _mockConfig.VerifyAll();
        _mockStateParser.VerifyAll();
        _mockHandoverParser.VerifyAll();
        _mockFileSystem.VerifyAll();
        _mockNavigation.VerifyAll();
    }

    /// <summary>
    /// Verifies that all data is populated when both STATE.md and handovers exist.
    /// </summary>
    [Test]
    public void LoadData_WhenAllFilesExist_ShouldPopulateAllData()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        _mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(true);
        _mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(true);

        var phaseProgressItem = new PhaseProgressItem { PhaseName = "Pre-Phase", Status = "Done" };

        _mockStateParser.Setup(p => p.Parse(statePath)).Returns(new ProjectState
        {
            CurrentPhase = "Phase 1",
            Day = 3,
            LastUpdated = "2026-03-23",
            Branch = "features/restructure",
            NextActions = [new ActionItem { Number = 1, Description = "Task 1", Status = ActionStatus.Next }],
            PhaseProgress = [phaseProgressItem],
            OpenDecisions = [new OpenDecision { Description = "Decision A", Impact = ImpactLevel.High }],
            TeamMembers = ["Chris"]
        });

        _mockHandoverParser.Setup(p => p.ParseAll(handoversDir)).Returns(
        [
            new HandoverNotes
            {
                DeveloperName = "Chris",
                ForNextSession = ["Continue Phase 1", "Review ADRs"]
            }
        ]);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.IsLoaded.Should().BeTrue();
        vm.ErrorMessage.Should().BeNull();
        vm.CurrentPhase.Should().Be("Phase 1");
        vm.CurrentDay.Should().Be(3);
        vm.LastUpdated.Should().Be("2026-03-23");
        vm.Branch.Should().Be("features/restructure");
        vm.NextActions.Should().HaveCount(1);
        vm.PendingActionCount.Should().Be(1);
        vm.PhaseProgress.Should().HaveCount(1);
        vm.OpenDecisions.Should().HaveCount(1);
        vm.OpenDecisionCount.Should().Be(1);
        vm.HandoverDeveloper.Should().Be("CHRIS");
        vm.HandoverNextItems.Should().HaveCount(2);
        vm.TeamMembers.Should().Contain("Chris");
        vm.SelectedPhase.Should().BeSameAs(phaseProgressItem);
    }

    /// <summary>
    /// Verifies that handover data is still loaded when STATE.md is missing.
    /// </summary>
    [Test]
    public void LoadData_WhenStateFileMissing_ShouldStillLoadHandoverData()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        _mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(false);
        _mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(true);

        _mockHandoverParser.Setup(p => p.ParseAll(handoversDir)).Returns(
        [
            new HandoverNotes { DeveloperName = "Chris", ForNextSession = ["Item 1"] }
        ]);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.IsLoaded.Should().BeTrue();
        vm.CurrentPhase.Should().BeEmpty();
        vm.SelectedPhase.Should().BeNull();
        vm.HandoverDeveloper.Should().Be("CHRIS");
    }

    /// <summary>
    /// Verifies that an error message is set and IsLoaded remains false when an exception is thrown.
    /// </summary>
    [Test]
    public void LoadData_WhenExceptionThrown_ShouldSetErrorMessage()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Throws(new InvalidOperationException("Config not set"));

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.IsLoaded.Should().BeFalse();
        vm.ErrorMessage.Should().Contain("Config not set");
    }

    /// <summary>
    /// Verifies that the first phase is auto-selected after loading state data.
    /// </summary>
    [Test]
    public void LoadData_WhenStateHasPhases_ShouldAutoSelectFirstPhase()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        _mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(true);
        _mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(false);

        var firstPhase = new PhaseProgressItem { PhaseName = "Pre-Phase", Status = "Done" };
        var secondPhase = new PhaseProgressItem { PhaseName = "Phase 1", Status = "In Progress" };

        _mockStateParser.Setup(p => p.Parse(statePath)).Returns(new ProjectState
        {
            CurrentPhase = "Phase 1",
            Day = 2,
            LastUpdated = "2026-03-24",
            Branch = "features/restructure",
            NextActions = [],
            PhaseProgress = [firstPhase, secondPhase],
            OpenDecisions = [],
            TeamMembers = []
        });

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.SelectedPhase.Should().BeSameAs(firstPhase);
        vm.PhaseProgress.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that SelectedPhase is null when the state has no phases.
    /// </summary>
    [Test]
    public void LoadData_WhenStateHasNoPhases_ShouldSetSelectedPhaseToNull()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        _mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(true);
        _mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(false);

        _mockStateParser.Setup(p => p.Parse(statePath)).Returns(new ProjectState
        {
            CurrentPhase = "Phase 1",
            Day = 1,
            LastUpdated = "2026-03-24",
            Branch = "features/restructure",
            NextActions = [],
            PhaseProgress = [],
            OpenDecisions = [],
            TeamMembers = []
        });

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.SelectedPhase.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the handover developer name is converted to upper-case invariant.
    /// </summary>
    [Test]
    public void LoadData_WhenHandoverExists_ShouldUppercaseDeveloperName()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        _mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(false);
        _mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(true);

        _mockHandoverParser.Setup(p => p.ParseAll(handoversDir)).Returns(
        [
            new HandoverNotes { DeveloperName = "alice", ForNextSession = ["Task A"] }
        ]);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.HandoverDeveloper.Should().Be("ALICE");
    }

    /// <summary>
    /// Verifies that handover data is not populated when the handovers directory does not exist.
    /// </summary>
    [Test]
    public void LoadData_WhenHandoversDirectoryMissing_ShouldNotPopulateHandoverData()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        _mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(false);
        _mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(false);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.IsLoaded.Should().BeTrue();
        vm.HandoverDeveloper.Should().BeEmpty();
        vm.HandoverNextItems.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when configurationService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullConfigService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DashboardViewModel(
            null!,
            _mockStateParser.Object,
            _mockHandoverParser.Object,
            _mockFileSystem.Object,
            _mockNavigation.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("configurationService");
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when stateParser is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullStateParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DashboardViewModel(
            _mockConfig.Object,
            null!,
            _mockHandoverParser.Object,
            _mockFileSystem.Object,
            _mockNavigation.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("stateParser");
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when handoverParser is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullHandoverParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DashboardViewModel(
            _mockConfig.Object,
            _mockStateParser.Object,
            null!,
            _mockFileSystem.Object,
            _mockNavigation.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("handoverParser");
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when fileSystemService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DashboardViewModel(
            _mockConfig.Object,
            _mockStateParser.Object,
            _mockHandoverParser.Object,
            null!,
            _mockNavigation.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fileSystemService");
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when navigationService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullNavigationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DashboardViewModel(
            _mockConfig.Object,
            _mockStateParser.Object,
            _mockHandoverParser.Object,
            _mockFileSystem.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("navigationService");
    }

    /// <summary>
    /// Verifies that OpenPhaseDocument navigates to the PlanFilePath of the provided phase.
    /// </summary>
    [Test]
    public void OpenPhaseDocument_WhenPhaseProvided_ShouldNavigateToFile()
    {
        // Arrange
        var planFilePath = @"C:\root\plans\phase1.md";
        var phase = new PhaseProgressItem { PhaseName = "Phase 1", PlanFilePath = planFilePath };
        _mockNavigation.Setup(n => n.NavigateToFile(planFilePath));

        var vm = CreateViewModel();

        // Act
        vm.OpenPhaseDocument(phase);

        // Assert
        _mockNavigation.Verify(n => n.NavigateToFile(planFilePath), Times.Once);
    }

    /// <summary>
    /// Verifies that OpenPhaseDocument falls back to SelectedPhase when null is passed.
    /// </summary>
    [Test]
    public void OpenPhaseDocument_WhenNull_ShouldUseSelectedPhase()
    {
        // Arrange
        var planFilePath = @"C:\root\plans\pre-phase.md";
        var selectedPhase = new PhaseProgressItem { PhaseName = "Pre-Phase", PlanFilePath = planFilePath };
        _mockNavigation.Setup(n => n.NavigateToFile(planFilePath));

        var vm = CreateViewModel();
        vm.SelectedPhase = selectedPhase;

        // Act
        vm.OpenPhaseDocument(null);

        // Assert
        _mockNavigation.Verify(n => n.NavigateToFile(planFilePath), Times.Once);
    }

    /// <summary>
    /// Creates a <see cref="DashboardViewModel"/> with all mock dependencies.
    /// </summary>
    private DashboardViewModel CreateViewModel()
    {
        return new DashboardViewModel(
            _mockConfig.Object,
            _mockStateParser.Object,
            _mockHandoverParser.Object,
            _mockFileSystem.Object,
            _mockNavigation.Object);
    }
}
