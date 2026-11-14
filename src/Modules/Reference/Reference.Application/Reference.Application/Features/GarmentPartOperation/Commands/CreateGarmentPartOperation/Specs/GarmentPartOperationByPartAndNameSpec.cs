using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Reference.Domain.GarmentParts;

namespace Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation.Specs;

public sealed class GarmentPartOperationByPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameSpec(int garmentPartId, string name)
    {
        Query.Where(x => x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name);
    }
}
