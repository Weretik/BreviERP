using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPartOperation.Update.Specifications;

public sealed class GarmentPartByNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
