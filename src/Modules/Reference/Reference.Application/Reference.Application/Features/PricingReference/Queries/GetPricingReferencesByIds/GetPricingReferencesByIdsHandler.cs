using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;
using FabricEntity = Reference.Domain.Fabrics.Fabric;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

public sealed class GetPricingReferencesByIdsHandler(
    IReferenceReadRepository<FabricEntity> fabricRepository,
    IReferenceReadRepository<GarmentAccessoryEntity> garmentAccessoryRepository,
    IReferenceReadRepository<GarmentPartOperationEntity> garmentPartOperationRepository,
    IReferenceReadRepository<PricingParameterEntity> pricingParameterRepository)
    : IQueryHandler<GetPricingReferencesByIdsQuery, PricingReferenceSnapshot>
{
    public async ValueTask<PricingReferenceSnapshot> Handle(
        GetPricingReferencesByIdsQuery query,
        CancellationToken cancellationToken)
    {
        var fabrics = query.FabricIds.Count == 0
            ? []
            : await fabricRepository.ListAsync(new PricingFabricsByIdsSpec(query.FabricIds), cancellationToken);
        var accessories = query.GarmentAccessoryIds.Count == 0
            ? []
            : await garmentAccessoryRepository.ListAsync(
                new PricingGarmentAccessoriesByIdsSpec(query.GarmentAccessoryIds), cancellationToken);
        var operations = query.GarmentPartOperationIds.Count == 0
            ? []
            : await garmentPartOperationRepository.ListAsync(
                new PricingGarmentPartOperationsByIdsSpec(query.GarmentPartOperationIds), cancellationToken);
        var pricingParameters = query.PricingParameterIds.Count == 0 && query.PricingParameterCodes.Count == 0
            ? []
            : await pricingParameterRepository.ListAsync(
                new PricingParametersByIdsOrCodesSpec(
                    query.PricingParameterIds,
                    query.PricingParameterCodes),
                cancellationToken);

        return new PricingReferenceSnapshot(fabrics, accessories, operations, pricingParameters);
    }
}
