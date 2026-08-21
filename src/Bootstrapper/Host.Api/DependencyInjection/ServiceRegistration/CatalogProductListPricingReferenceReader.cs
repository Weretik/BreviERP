using Catalog.Application.Contracts.Reference;
using Mediator;
using Reference.Application.Features.PricingReference.GetByIds;

namespace Host.Api.DependencyInjection.ServiceRegistration;

internal sealed class CatalogProductListPricingReferenceReader(ISender sender) : IProductListPricingReferenceReader
{
    public async Task<ProductListPricingReferenceData> GetAsync(
        ProductListPricingReferenceRequest request,
        CancellationToken cancellationToken)
    {
        var snapshot = await sender.Send(
            new GetPricingReferencesByIdsQuery(
                request.FabricIds,
                request.GarmentAccessoryIds,
                request.GarmentPartOperationIds,
                request.AdditionalReferenceIds,
                request.AdditionalReferenceKeys),
            cancellationToken);

        return new ProductListPricingReferenceData(
            snapshot.Fabrics.ToDictionary(x => x.Id, x => x.Value),
            snapshot.GarmentAccessories.ToDictionary(x => x.Id, x => x.Value),
            snapshot.GarmentPartOperations.ToDictionary(x => x.Id, x => x.Value),
            snapshot.AdditionalReferences.ToDictionary(
                x => x.Id,
                x => new ProductListAdditionalReference(x.Id, x.Key!, x.Value, x.Unit!)));
    }
}
