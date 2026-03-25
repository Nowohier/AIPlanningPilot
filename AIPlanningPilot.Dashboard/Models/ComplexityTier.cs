namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents the complexity tier of an entity for migration effort estimation.
/// </summary>
public enum ComplexityTier
{
    /// <summary>Simple entity with 8 or fewer properties and standard CRUD.</summary>
    Simple,

    /// <summary>Medium entity with 8-20 properties and multiple foreign keys.</summary>
    Medium,

    /// <summary>Complex entity with 20+ properties or state machines.</summary>
    Complex,

    /// <summary>Very complex entity with state machines, prefill, and custom handlers.</summary>
    VeryComplex
}
