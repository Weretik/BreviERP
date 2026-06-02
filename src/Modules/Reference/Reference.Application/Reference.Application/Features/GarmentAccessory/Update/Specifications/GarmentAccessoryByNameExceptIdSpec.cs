using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Update.Specifications;

public sealed class GarmentAccessoryByNameExceptIdSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value != id && x.Name == name);
    }
}
