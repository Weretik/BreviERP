using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation;

public sealed class GarmentPartOperationByIdOrPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByIdOrPartAndNameSpec(int id, int garmentPartId, string name)
    {
        Query.Where(x => x.Id == GarmentPartOperationId.From(id) || (x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name));
    }
}
