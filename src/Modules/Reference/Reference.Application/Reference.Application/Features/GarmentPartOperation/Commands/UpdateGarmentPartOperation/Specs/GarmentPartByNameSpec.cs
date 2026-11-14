using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;

namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation.Specs;

public sealed class GarmentPartByNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
