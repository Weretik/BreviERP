using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;

namespace Reference.Application.Features.GarmentAccessory.Create.Specifications;

public sealed class GarmentAccessoryByNameSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
