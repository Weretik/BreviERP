using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Create.Specifications;

public sealed class GarmentPartOperationByIdOrPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByIdOrPartAndNameSpec(int id, int garmentPartId, string name)
    {
        Query.Where(x => x.Id.Value == id || (x.GarmentPartId.Value == garmentPartId && x.Name == name));
    }
}
