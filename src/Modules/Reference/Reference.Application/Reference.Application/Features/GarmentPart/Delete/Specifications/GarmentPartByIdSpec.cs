using GarmentPartEntity = Reference.Domain.Entities.GarmentPart;

namespace Reference.Application.Features.GarmentPart.Delete.Specifications;

public sealed class GarmentPartByIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}
