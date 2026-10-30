namespace Crm.Application.CustomSewing.Production.Export;
public sealed record ProductionPackageRequest(Guid QuoteId, Guid VersionId, Guid RequestId,
    IReadOnlyList<Guid> VariantIds);
