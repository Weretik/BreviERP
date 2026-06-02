using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPartOperation.Delete.Specifications;

public sealed class GarmentPartOperationByIdSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}
