using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPart.Update.Specifications;

public sealed class GarmentPartByNameExceptIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != GarmentPartId.From(id) && x.Name == name);
    }
}
