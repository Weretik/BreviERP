using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using GarmentPartOperationId = Reference.Domain.GarmentPartOperations.GarmentPartOperationId;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

internal sealed class PricingGarmentPartOperationsByIdsSpec : Specification<GarmentPartOperationEntity, PricingReferenceItem>
{
    public PricingGarmentPartOperationsByIdsSpec(IReadOnlyCollection<int> ids)
    {
        var garmentPartOperationIds = ids.Select(GarmentPartOperationId.From).ToArray();

        Query.AsNoTracking()
            .Where(x => garmentPartOperationIds.Contains(x.Id))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Min));
    }
}
