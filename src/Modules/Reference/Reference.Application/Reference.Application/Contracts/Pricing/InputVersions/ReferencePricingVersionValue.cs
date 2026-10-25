namespace Reference.Application.Contracts.Pricing;

public sealed record ReferencePricingVersionValue(
    string ReferenceKey, long VersionId, decimal? Value, string Unit, bool IsActive, bool IsApproved, string SourceCode);

