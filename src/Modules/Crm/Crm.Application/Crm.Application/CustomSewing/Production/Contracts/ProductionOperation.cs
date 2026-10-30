namespace Crm.Application.CustomSewing.Production.Contracts;

public sealed record ProductionOperation(int ReferenceId, decimal Quantity, decimal? ManualMinutes,
    decimal? StandardMinutes, decimal EffectiveMinutes, long ReferenceVersionId);
