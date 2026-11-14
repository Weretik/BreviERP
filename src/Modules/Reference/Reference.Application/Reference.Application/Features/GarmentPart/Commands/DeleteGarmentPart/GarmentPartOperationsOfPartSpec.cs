using Reference.Domain.GarmentPartOperations;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;

namespace Reference.Application.Features.GarmentPart.Commands.DeleteGarmentPart;

public sealed class GarmentPartOperationsOfPartSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationsOfPartSpec(int garmentPartId) =>
        Query.Where(x => x.GarmentPartId == GarmentPartId.From(garmentPartId));
}
