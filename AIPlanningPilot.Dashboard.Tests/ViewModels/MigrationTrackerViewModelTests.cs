using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="MigrationTrackerViewModel"/>.
/// </summary>
[TestFixture]
public class MigrationTrackerViewModelTests
{
    private Mock<IConfigurationService> _mockConfig = null!;
    private Mock<IMigrationParser> _mockParser = null!;
    private Mock<IFileSystemService> _mockFileSystem = null!;

    /// <summary>
    /// Initializes mock dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        _mockParser = new Mock<IMigrationParser>(MockBehavior.Strict);
        _mockFileSystem = new Mock<IFileSystemService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _mockConfig.VerifyAll();
        _mockParser.VerifyAll();
        _mockFileSystem.VerifyAll();
    }

    /// <summary>
    /// Verifies that IsLoaded is set to true when the migration file exists and parsing succeeds.
    /// </summary>
    [Test]
    public void LoadData_WhenMigrationFileExists_ShouldSetIsLoadedTrue()
    {
        // Arrange
        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(new List<MigrationEntity>());

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.IsLoaded.Should().BeTrue();
        vm.HasMigrationFile.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasMigrationFile is false when the migration file does not exist.
    /// </summary>
    [Test]
    public void LoadData_WhenMigrationFileDoesNotExist_ShouldSetHasMigrationFileFalse()
    {
        // Arrange
        SetupConfigAndFileExists(false);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.HasMigrationFile.Should().BeFalse();
        vm.IsLoaded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an error message is set and IsLoaded remains false when the parser throws.
    /// </summary>
    [Test]
    public void LoadData_WhenParserThrows_ShouldSetErrorMessage()
    {
        // Arrange
        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Throws(new InvalidOperationException("Parse error"));

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.IsLoaded.Should().BeFalse();
        vm.ErrorMessage.Should().Contain("Parse error");
    }

    /// <summary>
    /// Verifies that KPI values are correctly computed after entities are loaded.
    /// </summary>
    [Test]
    public void LoadData_WhenEntitiesLoaded_ShouldComputeKpis()
    {
        // Arrange
        var entities = new List<MigrationEntity>
        {
            CreateEntity("Customer", "Sales", ComplexityTier.Simple, MigrationStatus.Done, hasManualCode: true),
            CreateEntity("Order", "Sales", ComplexityTier.Medium, MigrationStatus.InProgress),
            CreateEntity("Product", "Catalog", ComplexityTier.Complex, MigrationStatus.NotStarted),
            CreateEntity("Category", "Catalog", ComplexityTier.VeryComplex, MigrationStatus.Done)
        };

        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(entities);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.TotalEntitiesWithUi.Should().Be(4);
        vm.MigratedCount.Should().Be(2);
        vm.InProgressCount.Should().Be(1);
        vm.NotStartedCount.Should().Be(1);
        vm.ManualOverrideCount.Should().Be(1);
        vm.OverallProgressPercent.Should().Be(50.0);
        vm.SimpleTierCount.Should().Be(1);
        vm.MediumTierCount.Should().Be(1);
        vm.ComplexTierCount.Should().Be(1);
        vm.VeryComplexTierCount.Should().Be(1);
        vm.DonePercent.Should().Be(50.0);
        vm.InProgressPercent.Should().Be(25.0);
        vm.RemainingPercent.Should().Be(25.0);
    }

    /// <summary>
    /// Verifies that domain groups are built after entities are loaded.
    /// </summary>
    [Test]
    public void LoadData_WhenEntitiesLoaded_ShouldBuildDomainGroups()
    {
        // Arrange
        var entities = new List<MigrationEntity>
        {
            CreateEntity("Customer", "Sales", ComplexityTier.Simple, MigrationStatus.Done),
            CreateEntity("Order", "Sales", ComplexityTier.Medium, MigrationStatus.InProgress),
            CreateEntity("Product", "Catalog", ComplexityTier.Complex, MigrationStatus.NotStarted)
        };

        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(entities);

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.DomainGroups.Should().HaveCount(2);
        vm.DomainGroups.Should().Contain(g => g.DomainName == "Catalog" && g.EntityCount == 1);
        vm.DomainGroups.Should().Contain(g => g.DomainName == "Sales" && g.EntityCount == 2);
    }

    /// <summary>
    /// Verifies that setting SearchText filters domain groups by entity name.
    /// </summary>
    [Test]
    public void ApplyFilters_WhenSearchTextSet_ShouldFilterByName()
    {
        // Arrange
        var entities = new List<MigrationEntity>
        {
            CreateEntity("Customer", "Sales", ComplexityTier.Simple, MigrationStatus.Done),
            CreateEntity("Order", "Sales", ComplexityTier.Medium, MigrationStatus.InProgress),
            CreateEntity("Product", "Catalog", ComplexityTier.Complex, MigrationStatus.NotStarted)
        };

        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(entities);

        var vm = CreateViewModel();
        vm.LoadData();

        // Act
        vm.SearchText = "Customer";

        // Assert
        vm.DomainGroups.Should().HaveCount(1);
        vm.DomainGroups[0].DomainName.Should().Be("Sales");
        vm.DomainGroups[0].EntityCount.Should().Be(1);
        vm.DomainGroups[0].Entities.Should().ContainSingle(e => e.Name == "Customer");
    }

    /// <summary>
    /// Verifies that setting SelectedStatusFilter filters domain groups by migration status.
    /// </summary>
    [Test]
    public void ApplyFilters_WhenStatusFilterSet_ShouldFilterByStatus()
    {
        // Arrange
        var entities = new List<MigrationEntity>
        {
            CreateEntity("Customer", "Sales", ComplexityTier.Simple, MigrationStatus.Done),
            CreateEntity("Order", "Sales", ComplexityTier.Medium, MigrationStatus.InProgress),
            CreateEntity("Product", "Catalog", ComplexityTier.Complex, MigrationStatus.NotStarted)
        };

        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(entities);

        var vm = CreateViewModel();
        vm.LoadData();

        // Act
        vm.SelectedStatusFilter = "Done";

        // Assert
        vm.DomainGroups.Should().HaveCount(1);
        vm.DomainGroups[0].DomainName.Should().Be("Sales");
        vm.DomainGroups[0].Entities.Should().ContainSingle(e => e.Name == "Customer");
    }

    /// <summary>
    /// Verifies that setting SelectedComplexityFilter filters domain groups by complexity tier.
    /// </summary>
    [Test]
    public void ApplyFilters_WhenComplexityFilterSet_ShouldFilterByComplexity()
    {
        // Arrange
        var entities = new List<MigrationEntity>
        {
            CreateEntity("Customer", "Sales", ComplexityTier.Simple, MigrationStatus.Done),
            CreateEntity("Order", "Sales", ComplexityTier.Medium, MigrationStatus.InProgress),
            CreateEntity("Product", "Catalog", ComplexityTier.Complex, MigrationStatus.NotStarted)
        };

        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(entities);

        var vm = CreateViewModel();
        vm.LoadData();

        // Act
        vm.SelectedComplexityFilter = "Complex";

        // Assert
        vm.DomainGroups.Should().HaveCount(1);
        vm.DomainGroups[0].DomainName.Should().Be("Catalog");
        vm.DomainGroups[0].Entities.Should().ContainSingle(e => e.Name == "Product");
    }

    /// <summary>
    /// Verifies that KPIs are set to zero values when there are no entities.
    /// </summary>
    [Test]
    public void ComputeKpis_WhenNoEntities_ShouldSetZeroValues()
    {
        // Arrange
        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(new List<MigrationEntity>());

        var vm = CreateViewModel();

        // Act
        vm.LoadData();

        // Assert
        vm.TotalEntitiesWithUi.Should().Be(0);
        vm.MigratedCount.Should().Be(0);
        vm.InProgressCount.Should().Be(0);
        vm.NotStartedCount.Should().Be(0);
        vm.ManualOverrideCount.Should().Be(0);
        vm.OverallProgressPercent.Should().Be(0);
        vm.SimpleTierCount.Should().Be(0);
        vm.MediumTierCount.Should().Be(0);
        vm.ComplexTierCount.Should().Be(0);
        vm.VeryComplexTierCount.Should().Be(0);
        vm.DonePercent.Should().Be(0);
        vm.InProgressPercent.Should().Be(0);
        vm.RemainingPercent.Should().Be(100);
    }

    /// <summary>
    /// Verifies that the constructor throws when configurationService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullConfigService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MigrationTrackerViewModel(null!, _mockParser.Object, _mockFileSystem.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("configurationService");
    }

    /// <summary>
    /// Verifies that the constructor throws when migrationParser is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullMigrationParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MigrationTrackerViewModel(_mockConfig.Object, null!, _mockFileSystem.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("migrationParser");
    }

    /// <summary>
    /// Verifies that the constructor throws when fileSystemService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MigrationTrackerViewModel(_mockConfig.Object, _mockParser.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fileSystemService");
    }

    /// <summary>
    /// Sets up the configuration service and file system mocks for a standard LoadData call.
    /// </summary>
    /// <param name="fileExists">Whether the migration file should exist.</param>
    private void SetupConfigAndFileExists(bool fileExists)
    {
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        _mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(fileExists);
    }

    /// <summary>
    /// Creates a <see cref="MigrationEntity"/> for testing purposes.
    /// </summary>
    /// <param name="name">The entity name.</param>
    /// <param name="domain">The domain name.</param>
    /// <param name="complexity">The complexity tier.</param>
    /// <param name="status">The migration status.</param>
    /// <param name="hasManualCode">Whether the entity has manual code.</param>
    /// <returns>A configured <see cref="MigrationEntity"/> instance.</returns>
    private static MigrationEntity CreateEntity(
        string name,
        string domain,
        ComplexityTier complexity,
        MigrationStatus status,
        bool hasManualCode = false)
    {
        return new MigrationEntity
        {
            Name = name,
            Domain = domain,
            Complexity = complexity,
            Status = status,
            HasManualCode = hasManualCode,
            PropertyCount = 5,
            HasUi = true,
            Date = string.Empty
        };
    }

    /// <summary>
    /// Verifies that combined search text and status filter work together correctly.
    /// </summary>
    [Test]
    public void ApplyFilters_WhenCombinedFilters_ShouldFilterCorrectly()
    {
        // Arrange
        var entities = new List<MigrationEntity>
        {
            CreateEntity("Customer", "Sales", ComplexityTier.Simple, MigrationStatus.Done),
            CreateEntity("CustomerAddress", "Sales", ComplexityTier.Medium, MigrationStatus.InProgress),
            CreateEntity("Order", "Sales", ComplexityTier.Medium, MigrationStatus.Done),
            CreateEntity("Product", "Catalog", ComplexityTier.Complex, MigrationStatus.NotStarted)
        };

        SetupConfigAndFileExists(true);
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(entities);

        var vm = CreateViewModel();
        vm.LoadData();

        // Act
        vm.SearchText = "Customer";
        vm.SelectedStatusFilter = "Done";

        // Assert
        vm.DomainGroups.Should().HaveCount(1);
        vm.DomainGroups[0].DomainName.Should().Be("Sales");
        vm.DomainGroups[0].Entities.Should().ContainSingle(e => e.Name == "Customer");
    }

    /// <summary>
    /// Creates a <see cref="MigrationTrackerViewModel"/> with all mock dependencies.
    /// </summary>
    private MigrationTrackerViewModel CreateViewModel()
    {
        return new MigrationTrackerViewModel(
            _mockConfig.Object,
            _mockParser.Object,
            _mockFileSystem.Object);
    }
}
