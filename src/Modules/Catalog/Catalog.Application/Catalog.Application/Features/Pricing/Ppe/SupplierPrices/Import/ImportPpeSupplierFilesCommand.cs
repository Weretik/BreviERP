using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;

/// <summary>
/// Reads a file supplier's price files and shows what changes (<paramref name="DryRun"/>), or saves the changed
/// prices as new approved offer versions. The staff member sends the same files again to apply.
/// </summary>
public sealed record ImportPpeSupplierFilesCommand(
    string Key,
    IReadOnlyList<SupplierPriceFile> Files,
    bool DryRun,
    string Actor,
    string ActorName) : ICommand<Result<PpePriceImportResult>>;

public sealed record PpePriceImportResult(
    string SourceKey,
    bool Applied,
    decimal DiscountPercent,
    int Checked,
    int Changed,
    int Unchanged,
    int Skipped,
    IReadOnlyList<PpePriceImportRow> Rows);
