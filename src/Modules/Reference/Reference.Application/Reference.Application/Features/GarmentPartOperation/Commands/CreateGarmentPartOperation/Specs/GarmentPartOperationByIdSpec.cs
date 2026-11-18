using BuildingBlocks.Domain.Identifiers.ReferenceData;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation.Specs;

public sealed class GarmentPartOperationByIdSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByIdSpec(int id)
    {
        Query.Where(x => x.Id == new GarmentPartOperationId(id));
    }
}
