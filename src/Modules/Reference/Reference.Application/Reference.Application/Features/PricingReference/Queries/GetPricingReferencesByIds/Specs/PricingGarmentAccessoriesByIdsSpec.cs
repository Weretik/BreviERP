using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using GarmentAccessoryId = BuildingBlocks.Domain.Identifiers.ReferenceData.GarmentAccessoryId;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds.Specs;

internal sealed class PricingGarmentAccessoriesByIdsSpec : Specification<GarmentAccessoryEntity, PricingReferenceItem>
{
    public PricingGarmentAccessoriesByIdsSpec(IReadOnlyCollection<int> ids)
    {
        var garmentAccessoryIds = ids.Select(x => new GarmentAccessoryId(x)).ToArray();

        Query.AsNoTracking()
            .Where(x => garmentAccessoryIds.Contains(x.Id))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Price.Value));
    }
}
