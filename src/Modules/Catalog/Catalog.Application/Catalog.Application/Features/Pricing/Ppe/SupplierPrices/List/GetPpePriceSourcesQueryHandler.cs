using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Shared;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.List;

public sealed class GetPpePriceSourcesQueryHandler(
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
