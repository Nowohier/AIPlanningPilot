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
    private Mock<IConfigurationService> mockConfig = null!;
    private Mock<IStateParser> mockStateParser = null!;
    private Mock<IHandoverParser> mockHandoverParser = null!;
    private Mock<IFileSystemService> mockFileSystem = null!;
    private Mock<INavigationService> mockNavigation = null!;

    /// <summary>
    /// Initializes mock dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockStateParser = new Mock<IStateParser>(MockBehavior.Strict);
        mockHandoverParser = new Mock<IHandoverParser>(MockBehavior.Strict);
        mockFileSystem = new Mock<IFileSystemService>(MockBehavior.Strict);
        mockNavigation = new Mock<INavigationService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        mockConfig.VerifyAll();
        mockStateParser.VerifyAll();
        mockHandoverParser.VerifyAll();
        mockFileSystem.VerifyAll();
        mockNavigation.VerifyAll();
    }

    /// <summary>
    /// Verifies that all data is populated when both STATE.md and handovers exist.
    /// </summary>
    [Test]
    public void LoadData_WhenAllFilesExist_ShouldPopulateAllData()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(true);
        mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(true);

        var phaseProgressItem = new PhaseProgressItem { PhaseName = "Pre-Phase", Status = "Done" };

        mockStateParser.Setup(p => p.Parse(statePath)).Returns(new ProjectState
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

        mockHandoverParser.Setup(p => p.ParseAll(handoversDir)).Returns(
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
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(false);
        mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(true);

        mockHandoverParser.Setup(p => p.ParseAll(handoversDir)).Returns(
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
        mockConfig.Setup(c => c.RestructuringRootPath).Throws(new InvalidOperationException("Config not set"));

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
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(true);
        mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(false);

        var firstPhase = new PhaseProgressItem { PhaseName = "Pre-Phase", Status = "Done" };
        var secondPhase = new PhaseProgressItem { PhaseName = "Phase 1", Status = "In Progress" };

        mockStateParser.Setup(p => p.Parse(statePath)).Returns(new ProjectState
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
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(true);
        mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(false);

        mockStateParser.Setup(p => p.Parse(statePath)).Returns(new ProjectState
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
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(false);
        mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(true);

        mockHandoverParser.Setup(p => p.ParseAll(handoversDir)).Returns(
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
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        var statePath = Path.Combine(@"C:\root", "main", "STATE.md");
        var handoversDir = Path.Combine(@"C:\root", "handovers");

        mockFileSystem.Setup(fs => fs.FileExists(statePath)).Returns(false);
        mockFileSystem.Setup(fs => fs.DirectoryExists(handoversDir)).Returns(false);

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
            mockStateParser.Object,
            mockHandoverParser.Object,
            mockFileSystem.Object,
            mockNavigation.Object);

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
            mockConfig.Object,
            null!,
            mockHandoverParser.Object,
            mockFileSystem.Object,
            mockNavigation.Object);

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
            mockConfig.Object,
            mockStateParser.Object,
            null!,
            mockFileSystem.Object,
            mockNavigation.Object);

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
            mockConfig.Object,
            mockStateParser.Object,
            mockHandoverParser.Object,
            null!,
            mockNavigation.Object);

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
            mockConfig.Object,
            mockStateParser.Object,
            mockHandoverParser.Object,
            mockFileSystem.Object,
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
        mockNavigation.Setup(n => n.NavigateToFile(planFilePath));

        var vm = CreateViewModel();

        // Act
        vm.OpenPhaseDocument(phase);

        // Assert
        mockNavigation.Verify(n => n.NavigateToFile(planFilePath), Times.Once);
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
        mockNavigation.Setup(n => n.NavigateToFile(planFilePath));

        var vm = CreateViewModel();
        vm.SelectedPhase = selectedPhase;

        // Act
        vm.OpenPhaseDocument(null);

        // Assert
        mockNavigation.Verify(n => n.NavigateToFile(planFilePath), Times.Once);
    }

    /// <summary>
    /// Creates a <see cref="DashboardViewModel"/> with all mock dependencies.
    /// </summary>
    private DashboardViewModel CreateViewModel()
    {
        return new DashboardViewModel(
            mockConfig.Object,
            mockStateParser.Object,
            mockHandoverParser.Object,
            mockFileSystem.Object,
            mockNavigation.Object);
    }
}
