using GarmentPartOperationEntity = Reference.Domain.Entities.GarmentPartOperation;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.GarmentPartOperation.Update.Specifications;

public sealed class GarmentPartOperationByIdSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByIdSpec(int id)
    {
        Query.Where(x => x.Id == GarmentPartOperationId.From(id));
    }
}
