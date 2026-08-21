using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingReference.GetByIds.Specifications;
using AdditionalReferenceEntity = Reference.Domain.AdditionalReferences.Entities.AdditionalReference;
using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.PricingReference.GetByIds;

public sealed class GetPricingReferencesByIdsQueryHandler(
    IReferenceReadRepository<FabricEntity> fabricRepository,
    IReferenceReadRepository<GarmentAccessoryEntity> garmentAccessoryRepository,
    IReferenceReadRepository<GarmentPartOperationEntity> garmentPartOperationRepository,
    IReferenceReadRepository<AdditionalReferenceEntity> additionalReferenceRepository)
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
        var additionalReferences = query.AdditionalReferenceIds.Count == 0 && query.AdditionalReferenceKeys.Count == 0
            ? []
            : await additionalReferenceRepository.ListAsync(
                new PricingAdditionalReferencesByIdsOrKeysSpec(
                    query.AdditionalReferenceIds,
                    query.AdditionalReferenceKeys),
                cancellationToken);

        return new PricingReferenceSnapshot(fabrics, accessories, operations, additionalReferences);
    }
}
