using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

namespace Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

public sealed class GetPpePriceSourcesHandler(
    IPpePriceSourceStore store,
    IProductReferenceReader references,
    ICatalogReadRepository<ProductEntity> products)
    : IQueryHandler<GetPpePriceSourcesQuery, Result<IReadOnlyList<PpePriceSourceView>>>
{
    public async ValueTask<Result<IReadOnlyList<PpePriceSourceView>>> Handle(
        GetPpePriceSourcesQuery query, CancellationToken cancellationToken)
    {
        var settings = (await store.ReadSettingsAsync(cancellationToken)).ToDictionary(x => x.Key, StringComparer.Ordinal);
        var runs = (await store.ReadLastRunsAsync(cancellationToken)).ToDictionary(x => x.SourceKey, StringComparer.Ordinal);
        var suppliers = (await references.GetSnapshotAsync(cancellationToken)).Suppliers;
        var counts = (await products.ListAsync(new PpeSupplierCountsSpec(), cancellationToken))
            .GroupBy(row => row.SupplierId.Value)
            .ToDictionary(group => group.Key, group => (Total: group.Count(), Manual: group.Count(x => x.AllowManualPrice)));

        IReadOnlyList<PpePriceSourceView> views = PpePriceSources.All.Select(source =>
        {
            var saved = settings.GetValueOrDefault(source.Key);
            var supplier = PpePriceSourceSupplier.Resolve(source, saved, suppliers);
            var count = supplier is null ? default : counts.GetValueOrDefault(supplier.Id);
            return new PpePriceSourceView(source.Key, source.Title, source.Method, source.Schedule, source.Logic,
                saved?.DiscountPercent ?? source.DefaultDiscountPercent, source.DefaultDiscountPercent,
                supplier?.Id, supplier?.Name, count.Total, count.Manual, source.FileKind, source.FileCount,
                source.AutomaticUpdateConnected, saved?.UpdatedAt, saved?.UpdatedBy, runs.GetValueOrDefault(source.Key));
        }).ToList();
        return Result.Success(views);
    }
}
