using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.Entities.GarmentAccessory;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.GarmentAccessory.Update.Specifications;

public sealed class GarmentAccessoryByNameExceptIdSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != GarmentAccessoryId.From(id) && x.Name == name);
    }
}
