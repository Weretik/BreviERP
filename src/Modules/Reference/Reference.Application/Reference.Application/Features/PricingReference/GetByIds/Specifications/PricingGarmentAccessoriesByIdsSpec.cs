using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;
using GarmentAccessoryId = Reference.Domain.GarmentAccessories.ValueObjects.GarmentAccessoryId;

namespace Reference.Application.Features.PricingReference.GetByIds.Specifications;

internal sealed class PricingGarmentAccessoriesByIdsSpec : Specification<GarmentAccessoryEntity, PricingReferenceItem>
{
    public PricingGarmentAccessoriesByIdsSpec(IReadOnlyCollection<int> ids)
    {
        var garmentAccessoryIds = ids.Select(GarmentAccessoryId.From).ToArray();

        Query.AsNoTracking()
            .Where(x => garmentAccessoryIds.Contains(x.Id))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Price.Value));
    }
}
