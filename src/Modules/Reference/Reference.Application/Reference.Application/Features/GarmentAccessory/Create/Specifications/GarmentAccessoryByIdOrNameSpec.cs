using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentAccessory.Create.Specifications;

public sealed class GarmentAccessoryByIdOrNameSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == GarmentAccessoryId.From(id) || x.Name == name);
    }
}
