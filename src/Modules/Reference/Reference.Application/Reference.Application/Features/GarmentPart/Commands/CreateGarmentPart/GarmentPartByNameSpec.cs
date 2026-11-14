using GarmentPartEntity = Reference.Domain.GarmentPartOperations.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Commands.CreateGarmentPart;

public sealed class GarmentPartByNameSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
