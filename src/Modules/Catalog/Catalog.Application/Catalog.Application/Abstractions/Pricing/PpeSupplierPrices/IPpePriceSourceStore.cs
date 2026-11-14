namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

public interface IPpePriceSourceStore
{
    Task<IReadOnlyList<PpePriceSourceSettings>> ReadSettingsAsync(CancellationToken cancellationToken);

    Task SaveSettingsAsync(PpePriceSourceSettings settings, CancellationToken cancellationToken);

    /// <summary>The latest run of each source.</summary>
    Task<IReadOnlyList<PpePriceImportRun>> ReadLastRunsAsync(CancellationToken cancellationToken);

    Task RecordRunAsync(PpePriceImportRun run, CancellationToken cancellationToken);
}
