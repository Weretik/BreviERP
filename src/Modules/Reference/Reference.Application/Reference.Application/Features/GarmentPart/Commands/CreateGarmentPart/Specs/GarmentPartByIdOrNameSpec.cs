using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Reference.Domain.GarmentParts;

namespace Reference.Application.Features.GarmentPart.Commands.CreateGarmentPart.Specs;

public sealed class GarmentPartByIdOrNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == GarmentPartId.From(id) || x.Name == name);
    }
}
