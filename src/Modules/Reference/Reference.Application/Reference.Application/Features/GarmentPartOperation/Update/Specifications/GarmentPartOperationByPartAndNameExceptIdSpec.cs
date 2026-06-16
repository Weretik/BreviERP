using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPartOperation.Update.Specifications;

public sealed class GarmentPartOperationByPartAndNameExceptIdSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameExceptIdSpec(int id, int garmentPartId, string name)
    {
        Query.Where(x => x.Id != GarmentPartOperationId.From(id) && x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name);
    }
}
