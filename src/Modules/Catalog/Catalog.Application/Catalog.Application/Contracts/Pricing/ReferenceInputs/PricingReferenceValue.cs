namespace Catalog.Application.Contracts.Pricing;

public sealed record PricingReferenceValue(
    string ReferenceKey, long VersionId, decimal? Value, string Unit, bool IsActive, bool IsApproved, string SourceCode);

