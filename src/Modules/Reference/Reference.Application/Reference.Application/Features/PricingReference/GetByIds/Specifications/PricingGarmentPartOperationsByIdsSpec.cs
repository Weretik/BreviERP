using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;
using GarmentPartOperationId = Reference.Domain.GarmentPartOperations.ValueObjects.GarmentPartOperationId;

namespace Reference.Application.Features.PricingReference.GetByIds.Specifications;

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
