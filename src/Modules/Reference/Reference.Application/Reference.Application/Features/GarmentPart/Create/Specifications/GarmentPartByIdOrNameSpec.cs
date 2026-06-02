using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Create.Specifications;

public sealed class GarmentPartByIdOrNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value == id || x.Name == name);
    }
}
