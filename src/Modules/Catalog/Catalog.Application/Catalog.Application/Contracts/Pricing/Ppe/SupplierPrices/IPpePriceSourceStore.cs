namespace Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;

/// <summary>Saved «Імпорт цін» settings of one source; a source without a row uses its defaults.</summary>
public sealed record PpePriceSourceSettings(
    string Key,
    int? SupplierId,
    decimal DiscountPercent,
    DateTimeOffset UpdatedAt,
    string UpdatedBy);

/// <summary>A problem found during an import, shown to the staff member with its reason.</summary>
public sealed record PpePriceImportProblem(string? ProductCode, string? ProductName, string Message);

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

public interface IPpePriceSourceStore
{
    Task<IReadOnlyList<PpePriceSourceSettings>> ReadSettingsAsync(CancellationToken cancellationToken);

    Task SaveSettingsAsync(PpePriceSourceSettings settings, CancellationToken cancellationToken);

    /// <summary>The latest run of each source.</summary>
    Task<IReadOnlyList<PpePriceImportRun>> ReadLastRunsAsync(CancellationToken cancellationToken);

    Task RecordRunAsync(PpePriceImportRun run, CancellationToken cancellationToken);
}
