using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Update.Specifications;

public sealed class GarmentAccessoryByIdSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}
