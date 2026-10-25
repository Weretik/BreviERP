namespace SewingPricing.Jobs.Bootstrap;

public sealed record PricingBootstrapReport(
    string Status, string SourceHash, string CatalogSnapshotHash, long SettingsVersionId, int ReferenceVersions);

