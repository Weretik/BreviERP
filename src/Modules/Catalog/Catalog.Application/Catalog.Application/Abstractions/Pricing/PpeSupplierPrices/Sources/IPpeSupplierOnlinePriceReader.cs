using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;

/// <summary>
/// Reads the current prices of a website or Google Sheets source (АВ-Центр, Перчатка-центр, 3C-Маркет) the way the
/// client's Apps Script did. A page that fails or has no price comes back as an item with its
/// <see cref="SupplierFilePrice.Problem"/>; a price list that cannot be loaded at all fails the whole read, so no
/// price changes from a partial list.
/// </summary>
public interface IPpeSupplierOnlinePriceReader
{
    Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        PpePriceSource source, IReadOnlyList<PpePriceImportProduct> products, CancellationToken cancellationToken);
}
