using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentPart.Commands.DeleteGarmentPart;

public sealed class GarmentPartByIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdSpec(int id)
    {
        Query.Where(x => x.Id == GarmentPartId.From(id));
    }
}
