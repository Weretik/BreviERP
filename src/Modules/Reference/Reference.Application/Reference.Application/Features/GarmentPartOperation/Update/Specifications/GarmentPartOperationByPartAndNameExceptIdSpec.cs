using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Update.Specifications;

public sealed class GarmentPartOperationByPartAndNameExceptIdSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameExceptIdSpec(int id, int garmentPartId, string name)
    {
        Query.Where(x => x.Id.Value != id && x.GarmentPartId.Value == garmentPartId && x.Name == name);
    }
}
