using FabricEntity = Reference.Domain.GarmentAccessories.Fabric;
using FabricId = Reference.Domain.GarmentAccessories.FabricId;

namespace Reference.Application.Features.PricingReference.Queries.GetPricingReferencesByIds;

internal sealed class PricingFabricsByIdsSpec : Specification<FabricEntity, PricingReferenceItem>
{
    public PricingFabricsByIdsSpec(IReadOnlyCollection<int> ids)
    {
        var fabricIds = ids.Select(FabricId.From).ToArray();

        Query.AsNoTracking()
            .Where(x => fabricIds.Contains(x.Id))
            .Select(x => new PricingReferenceItem(x.Id.Value, x.Price.Value));
    }
}
