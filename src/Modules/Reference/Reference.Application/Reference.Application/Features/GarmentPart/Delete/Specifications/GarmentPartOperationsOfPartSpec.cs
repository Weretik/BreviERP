using Reference.Domain.GarmentPartOperations.ValueObjects;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPart.Delete.Specifications;

public sealed class GarmentPartOperationsOfPartSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationsOfPartSpec(int garmentPartId) =>
        Query.Where(x => x.GarmentPartId == GarmentPartId.From(garmentPartId));
}
