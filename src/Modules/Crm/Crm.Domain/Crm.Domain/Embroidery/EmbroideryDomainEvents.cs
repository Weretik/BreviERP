using Crm.Domain.Embroidery.Saved;

namespace Crm.Domain.Embroidery;

/// <summary>A person saved an embroidery calculation (ACT-001); one moved from the old archive is not raised.</summary>
public sealed record EmbroideryCalculationSaved(EmbroiderySavedCalculation Calculation) : IDomainEvent;

/// <summary>The mockup of a saved calculation was set or cleared (ACT-001).</summary>
public sealed record EmbroideryMockupSet(EmbroiderySavedCalculation Calculation) : IDomainEvent;

/// <summary>A saved calculation was deleted (ACT-001).</summary>
public sealed record EmbroideryCalculationDeleted(long CalculationId, string Name) : IDomainEvent;

/// <summary>The embroidery rates were saved (ACT-001); <paramref name="Version"/> is the new version.</summary>
public sealed record EmbroiderySettingsChanged(int Version) : IDomainEvent;
