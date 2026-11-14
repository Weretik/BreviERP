using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Reference.Domain.GarmentParts;

namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation.Specs;

public sealed class GarmentPartOperationByPartAndNameExceptIdSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameExceptIdSpec(int id, int garmentPartId, string name)
    {
        Query.Where(x => x.Id != GarmentPartOperationId.From(id) && x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name);
    }
}
