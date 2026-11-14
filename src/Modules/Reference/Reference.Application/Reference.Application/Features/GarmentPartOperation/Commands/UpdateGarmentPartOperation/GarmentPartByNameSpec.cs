using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;

namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;

public sealed class GarmentPartByNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
