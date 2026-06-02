using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Update.Specifications;

public sealed class GarmentPartByNameExceptIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value != id && x.Name == name);
    }
}
