namespace Reference.Application.Abstractions;

public sealed record ReferencePricingVersionValue(
    string ReferenceKey, long VersionId, decimal? Value, string Unit, bool IsActive, bool IsApproved, string SourceCode);

