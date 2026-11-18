using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using GarmentPartOperationId = BuildingBlocks.Domain.Identifiers.ReferenceData.GarmentPartOperationId;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds.Specs;

internal sealed class PricingGarmentPartOperationsByIdsSpec : Specification<GarmentPartOperationEntity, PricingReferenceItem>
{
    public PricingGarmentPartOperationsByIdsSpec(IReadOnlyCollection<int> ids)
    {
        var garmentPartOperationIds = ids.Select(x => new GarmentPartOperationId(x)).ToArray();

        Query.AsNoTracking()
            .Where(x => garmentPartOperationIds.Contains(x.Id))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Min));
    }
}
