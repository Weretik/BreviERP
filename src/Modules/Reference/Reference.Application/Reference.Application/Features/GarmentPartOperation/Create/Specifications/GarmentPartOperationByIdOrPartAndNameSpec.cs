using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPartOperation.Create.Specifications;

public sealed class GarmentPartOperationByIdOrPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByIdOrPartAndNameSpec(int id, int garmentPartId, string name)
    {
        Query.Where(x => x.Id == GarmentPartOperationId.From(id) || (x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name));
    }
}
