using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentAccessory.Update.Specifications;

public sealed class GarmentAccessoryByIdSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByIdSpec(int id)
    {
        Query.Where(x => x.Id == GarmentAccessoryId.From(id));
    }
}
