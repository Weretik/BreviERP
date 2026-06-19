using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPartOperation.Create.Specifications;

public sealed class GarmentPartOperationByPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameSpec(int garmentPartId, string name)
    {
        Query.Where(x => x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name);
    }
}
