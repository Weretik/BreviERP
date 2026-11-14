using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentPart.Commands.UpdateGarmentPart;

public sealed class GarmentPartByNameExceptIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != GarmentPartId.From(id) && x.Name == name);
    }
}
