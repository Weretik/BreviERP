namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

/// <summary>One finished import of a source: who ran it, how, and what changed.</summary>
public sealed record PpePriceImportRun(
    string SourceKey,
    string Trigger,
    string Actor,
    DateTimeOffset FinishedAt,
    int Checked,
    int Changed,
    int Unchanged,
    int Skipped,
    IReadOnlyList<PpePriceImportProblem> Problems);
