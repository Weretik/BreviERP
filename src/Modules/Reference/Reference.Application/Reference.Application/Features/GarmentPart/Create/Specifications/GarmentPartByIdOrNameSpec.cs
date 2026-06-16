using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPart.Create.Specifications;

public sealed class GarmentPartByIdOrNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == GarmentPartId.From(id) || x.Name == name);
    }
}
