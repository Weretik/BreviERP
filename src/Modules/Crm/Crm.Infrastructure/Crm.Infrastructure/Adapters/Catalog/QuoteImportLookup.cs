using Catalog.Contracts.Products;
using Crm.Application.Quotes.Import;
using Reference.Contracts.Fabrics;
using Reference.Contracts.GarmentAccessories;
using Reference.Contracts.GarmentPartOperations;

namespace Crm.Infrastructure.Adapters.Catalog;

/// <summary>
/// Catalog products and Reference fabrics, accessories and operations by code for the transfer of old КП (R-25).
/// </summary>
internal sealed class QuoteImportLookup(
    ICatalogProductReader products,
    IFabricDirectory fabricDirectory,
    IGarmentAccessoryDirectory accessoryDirectory,
    IGarmentPartOperationDirectory operationDirectory) : IQuoteImportLookup
{
    public async Task<QuoteImportReferences> LoadAsync(CancellationToken cancellationToken)
    {
        var sewing = await ProductsAsync(CatalogProductKind.Sewing, cancellationToken);
        var ppe = await ProductsAsync(CatalogProductKind.Ppe, cancellationToken);
        var fabrics = await fabricDirectory.ListAsync(cancellationToken);
        var accessories = await accessoryDirectory.ListAsync(cancellationToken);
        var operations = await operationDirectory.ListAsync(cancellationToken);

        return new QuoteImportReferences(
            ByCode(sewing, x => x.Code, x => new QuoteImportModel(x.Id,
                x.Variants.Select(v => new QuoteImportVariant(v.ArticleId, v.FabricId, v.Active)).ToArray())),
            ByCode(ppe, x => x.Code, x => x.Id),
            ByCode(ppe, x => x.Name.Trim().ToLowerInvariant(), x => x.Id),
            ByCode(fabrics.OrderByDescending(x => x.IsActive), x => x.Code, x => new QuoteImportReference(x.Id, x.Name, "м")),
            ByCode(accessories.OrderByDescending(x => x.IsActive), x => x.Code, x => new QuoteImportReference(x.Id, x.Name, x.Unit)),
            ByCode(operations.OrderByDescending(x => x.IsActive), x => x.Code,
                x => new QuoteImportReference(x.Id, QuoteSewingCalculator.OperationName(x), "хв")));
    }

    /// <summary>The first row of each code wins; an active row comes before a switched-off one.</summary>
    private static Dictionary<string, TValue> ByCode<TRow, TValue>(IEnumerable<TRow> rows, Func<TRow, string?> code,
        Func<TRow, TValue> value)
    {
        var map = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
            if (code(row)?.Trim() is { Length: > 0 } key) map.TryAdd(key, value(row));
        return map;
    }

    private async Task<IReadOnlyList<CatalogProduct>> ProductsAsync(CatalogProductKind kind,
        CancellationToken cancellationToken) =>
        (await products.ListAsync(kind, null, cancellationToken)).OrderByDescending(x => x.IsActive).ToArray();
}
