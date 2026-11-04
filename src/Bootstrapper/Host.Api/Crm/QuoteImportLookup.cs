using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Domain.Products.Enums;
using Crm.Application.Quotes.Import;
using Mediator;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.GarmentAccessory.GetList;
using Reference.Application.Features.GarmentPartOperation.GetList;

namespace Host.Api.Crm;

/// <summary>
/// Catalog products and Reference fabrics, accessories and operations by code for the transfer of old КП (R-25),
/// through the Catalog and Reference queries.
/// </summary>
internal sealed class QuoteImportLookup(ISender sender) : IQuoteImportLookup
{
    public async Task<QuoteImportReferences> LoadAsync(CancellationToken cancellationToken)
    {
        var sewing = await ProductsAsync(ProductType.Sewing, cancellationToken);
        var ppe = await ProductsAsync(ProductType.Ppe, cancellationToken);
        var fabrics = (await sender.Send(new GetAllFabricQuery(), cancellationToken)).Value ?? [];
        var accessories = (await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken)).Value ?? [];
        var operations = (await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken)).Value ?? [];

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

    private async Task<IReadOnlyList<ProductOverviewItem>> ProductsAsync(ProductType type,
        CancellationToken cancellationToken)
    {
        var rows = new List<ProductOverviewItem>();
        for (var page = 1; ; page++)
        {
            var result = await sender.Send(new GetProductOverviewPageQuery(type, null, null, false, false,
                ProductOverviewSort.Code, false, page, GetProductOverviewPageQuery.MaxPageSize), cancellationToken);
            if (!result.IsSuccess) break;
            rows.AddRange(result.Value.Items);
            if (rows.Count >= result.Value.Total || result.Value.Items.Count == 0) break;
        }

        return rows.OrderByDescending(x => x.IsActive).ToArray();
    }
}
