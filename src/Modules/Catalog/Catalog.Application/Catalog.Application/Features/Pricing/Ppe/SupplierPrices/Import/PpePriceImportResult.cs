namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;

/// <summary>What an update of a source found or saved: counts and every product of the supplier with its old and new price.</summary>
public sealed record PpePriceImportResult(
    string SourceKey,
    bool Applied,
    decimal DiscountPercent,
    int Checked,
    int Changed,
    int Unchanged,
    int Skipped,
    IReadOnlyList<PpePriceImportRow> Rows);
