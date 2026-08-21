namespace Catalog.Application.Contracts.Reference;

public interface IProductListPricingReferenceReader
{
    Task<ProductListPricingReferenceData> GetAsync(
        ProductListPricingReferenceRequest request,
        CancellationToken cancellationToken);
}
