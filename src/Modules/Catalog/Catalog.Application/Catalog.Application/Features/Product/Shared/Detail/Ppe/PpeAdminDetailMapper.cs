using Catalog.Domain.Products;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Queries.GetAdminProductDetail;
using Catalog.Domain.Products.Ppe.Pricing;

namespace Catalog.Application.Features.Product.Shared.Detail.Ppe;

internal static class PpeAdminDetailMapper
{
    public static PpeAdminDetail? Map(PpeReadModel ppe, ProductReferenceData references)
    {
        if (!references.Suppliers.TryGetValue(ppe.SupplierId, out var supplier))
            return null;
        return new(new NamedReferenceAdminDetail(supplier.Id, supplier.Name), ppe.BasePrice,
            Percent(ppe.RetailSource, ppe.RetailAdditionalReferenceId, ppe.RetailCustomPercent, references),
            Percent(ppe.WholesaleSource, ppe.WholesaleAdditionalReferenceId, ppe.WholesaleCustomPercent, references), 0m, 0m);
    }

    private static PpePercentAdminDetail Percent(PricePercentSource source, int? referenceId, decimal? customPercent, ProductReferenceData references)
    {
        var reference = referenceId is { } id && references.AdditionalReferences.TryGetValue(id, out var item)
            ? new AdditionalReferenceAdminDetail(item.Id, item.Name, item.Key, item.Value, item.Unit) : null;
        return new(source == PricePercentSource.Reference ? PpePercentSource.Reference : PpePercentSource.Custom, reference, customPercent);
    }
}
