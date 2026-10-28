namespace Crm.Domain.CustomSewing.Pricing;

public sealed record CustomReferenceBasis(string Key, long VersionId, decimal? Value, string Unit, string SourceCode);
