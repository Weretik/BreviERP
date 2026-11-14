using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentPart.Commands.CreateGarmentPart;

public sealed class GarmentPartByIdOrNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == GarmentPartId.From(id) || x.Name == name);
    }
}
