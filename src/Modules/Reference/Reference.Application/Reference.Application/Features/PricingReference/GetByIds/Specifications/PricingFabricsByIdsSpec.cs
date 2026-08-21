using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;
using FabricId = Reference.Domain.GarmentAccessories.ValueObjects.FabricId;

namespace Reference.Application.Features.PricingReference.GetByIds.Specifications;

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
