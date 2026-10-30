namespace Crm.Application.CustomSewing.Production.Contracts;

/// <summary>Allowlisted internal production manifest; never serialize a custom price basis into it.</summary>
public sealed record ProductionPackageManifest(int SchemaVersion, string Kind,
    Guid QuotePublicId, Guid QuoteVersionPublicId, int QuoteVersionNumber, string QuoteNumber,
    string Currency, IReadOnlyList<ProductionOption> Options, IReadOnlyList<ProductionPackageFile> Files);
