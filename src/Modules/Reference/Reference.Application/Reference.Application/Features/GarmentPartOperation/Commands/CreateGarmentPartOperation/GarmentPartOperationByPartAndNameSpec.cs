using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation;

public sealed class GarmentPartOperationByPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameSpec(int garmentPartId, string name)
    {
        Query.Where(x => x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name);
    }
}
