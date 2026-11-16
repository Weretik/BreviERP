using BuildingBlocks.Domain.Events;

namespace Production.Domain.FabricNorms;

/// <summary>A fabric norm was added (<paramref name="Before"/> is null) or changed (ACT-001).</summary>
public sealed record FabricNormSaved(ModelFabricNorm Norm, FabricNormValues? Before) : IDomainEvent;

/// <summary>A fabric norm was deleted (ACT-001).</summary>
public sealed record FabricNormDeleted(int NormId, string ModelCode) : IDomainEvent;
