using GarmentAccessoryEntity = Reference.Domain.Entities.GarmentAccessory;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentAccessory.Update.Specifications;

public sealed class GarmentAccessoryByNameExceptIdSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != GarmentAccessoryId.From(id) && x.Name == name);
    }
}
