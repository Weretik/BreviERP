using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPart.Create.Specifications;

public sealed class GarmentPartByIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdSpec(int id)
    {
        Query.Where(x => x.Id == GarmentPartId.From(id));
    }
}
