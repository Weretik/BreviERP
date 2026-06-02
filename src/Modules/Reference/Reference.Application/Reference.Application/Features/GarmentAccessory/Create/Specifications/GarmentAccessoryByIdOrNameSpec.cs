using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Create.Specifications;

public sealed class GarmentAccessoryByIdOrNameSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value == id || x.Name == name);
    }
}
