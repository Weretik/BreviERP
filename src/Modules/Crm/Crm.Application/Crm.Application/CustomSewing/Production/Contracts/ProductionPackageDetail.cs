namespace Crm.Application.CustomSewing.Production.Contracts;
public sealed record ProductionPackageDetail(Guid PackageId, Guid QuoteId, Guid VersionId,
    int SchemaVersion, string Sha256, long Length, string ContentType, DateTimeOffset CreatedAt);
