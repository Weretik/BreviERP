using Catalog.Application.Contracts.Reference;

using Reference.Contracts.Fabrics;
using Reference.Contracts.GarmentAccessories;
using Reference.Contracts.GarmentPartOperations;
using Reference.Contracts.PricingParameters;
using Reference.Contracts.ProductSections;
using Reference.Contracts.Suppliers;

namespace Catalog.Infrastructure.Adapters.Reference;

/// <summary>Gives Catalog the Reference fabric names (CAT-013).</summary>
internal sealed class FabricNameReader(IFabricDirectory fabrics) : IFabricNameReader
{
    public async Task<IReadOnlyDictionary<int, string>> ReadAsync(CancellationToken cancellationToken) =>
        (await fabrics.ListAsync(cancellationToken)).ToDictionary(x => x.Id, x => x.Name);
}

/// <summary>Gives Catalog the Reference «Типи і розділи» sections.</summary>
internal sealed class ProductSectionReader(IProductSectionDirectory sections) : IProductSectionReader
{
    public async Task<ProductSectionItem?> FindAsync(int id, CancellationToken cancellationToken) =>
        (await ListAsync(cancellationToken)).FirstOrDefault(x => x.Id == id);

    public async Task<IReadOnlyList<ProductSectionItem>> ListAsync(CancellationToken cancellationToken) =>
        (await sections.ListAsync(cancellationToken))
        .Select(x => new ProductSectionItem(x.Id, x.Type, x.Name, x.IsActive)).ToList();
}

/// <summary>Gives Catalog the sheet codes and names of Reference items.</summary>
internal sealed class ProductReferenceCodeReader(IFabricDirectory fabrics, IGarmentAccessoryDirectory accessories,
    IGarmentPartOperationDirectory operations, ISupplierDirectory suppliers, IProductSectionDirectory sections)
    : IProductReferenceCodeReader
{
    public async Task<ProductReferenceCodes> ReadAsync(CancellationToken cancellationToken) =>
        new(
            Codes((await fabrics.ListAsync(cancellationToken)).Select(x => (x.Id, x.Code))),
            Codes((await accessories.ListAsync(cancellationToken)).Select(x => (x.Id, x.Code))),
            Codes((await operations.ListAsync(cancellationToken)).Select(x => (x.Id, x.Code))),
            (await suppliers.ListAsync(cancellationToken)).Select(x => new ReferenceItem(x.Id, x.Name)).ToList(),
            (await sections.ListAsync(cancellationToken))
            .Select(x => new ProductSectionItem(x.Id, x.Type, x.Name, x.IsActive)).ToList());

    private static List<ReferenceCodeItem> Codes(IEnumerable<(int Id, string? Code)> items) =>
        items.Where(x => !string.IsNullOrWhiteSpace(x.Code)).Select(x => new ReferenceCodeItem(x.Id, x.Code!)).ToList();
}

/// <summary>Gives Catalog a snapshot of the Reference items a product may point to.</summary>
internal sealed class ProductReferenceReader(ISupplierDirectory suppliers, IFabricDirectory fabrics,
    IGarmentAccessoryDirectory accessories, IGarmentPartOperationDirectory operations,
    IPricingParameterDirectory parameters) : IProductReferenceReader
{
    public async Task<ProductReferenceData> GetSnapshotAsync(CancellationToken cancellationToken) =>
        new(
            (await suppliers.ListAsync(cancellationToken)).ToDictionary(x => x.Id, x => new ReferenceItem(x.Id, x.Name)),
            (await fabrics.ListAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => new PricedReferenceItem(x.Id, x.Name, x.Price)),
            (await accessories.ListAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => new PricedReferenceItem(x.Id, x.Name, x.Price)),
            (await operations.ListAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => new OperationReferenceItem(x.Id, x.Name, x.Min)),
            (await parameters.ListAsync(cancellationToken)).ToDictionary(x => x.Id,
                x => new AdditionalReferenceItem(x.Id, x.Name, x.Code, x.Value ?? 0m, x.Unit)));
}
