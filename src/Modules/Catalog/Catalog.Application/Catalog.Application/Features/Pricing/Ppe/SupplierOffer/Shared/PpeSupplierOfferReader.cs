using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Shared;

/// <summary>Builds the card view from the product profile, the supplier name and the current pricing input.</summary>
public static class PpeSupplierOfferReader
{
    public static async Task<PpeSupplierOffer> ReadAsync(ProductEntity product, IProductReferenceReader references,
        IPpePricingInputReader inputs, CancellationToken cancellationToken)
    {
        var id = product.Id.Value;
        var input = (await inputs.ReadAsync([id], cancellationToken)).SingleOrDefault()
                    ?? new PpePricingProductInput(id, product.CatalogProfile?.IsActive ?? true, null, null);
        var pricing = PpeProductPricingEvaluator.Evaluate(input);
        var profile = product.CatalogProfile;
        var supplierId = profile?.SupplierId?.Value;
        string? supplierName = null;
        if (supplierId is { } supplier)
            supplierName = (await references.GetSnapshotAsync(cancellationToken)).Suppliers.GetValueOrDefault(supplier)?.Name;
        var offer = input.Offer;
        return new PpeSupplierOffer(id, profile?.Unit, supplierId, supplierName, profile?.SupplierSku,
            profile?.SupplierProductUrl, profile?.SupplierPrice ?? offer?.Values.SupplierCost,
            profile?.AllowManualPrice ?? false, profile?.PricingRuleCode ?? offer?.Values.RuleCode,
            offer?.Values.ExternalWholesale, offer?.Values.ExternalRetail, offer?.Id, offer?.CreatedAt,
            pricing.Calculation);
    }
}
