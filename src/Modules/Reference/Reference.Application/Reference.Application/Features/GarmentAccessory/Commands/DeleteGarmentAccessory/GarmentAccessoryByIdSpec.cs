using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;

public sealed class GarmentAccessoryByIdSpec : Specification<GarmentAccessoryEntity>
{
    public GarmentAccessoryByIdSpec(int id)
    {
        Query.Where(x => x.Id == GarmentAccessoryId.From(id));
    }
}
