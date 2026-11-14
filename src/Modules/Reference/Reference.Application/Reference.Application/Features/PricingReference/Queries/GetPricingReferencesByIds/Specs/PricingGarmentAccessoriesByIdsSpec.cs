using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using GarmentAccessoryId = Reference.Domain.GarmentAccessories.GarmentAccessoryId;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds.Specs;

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
