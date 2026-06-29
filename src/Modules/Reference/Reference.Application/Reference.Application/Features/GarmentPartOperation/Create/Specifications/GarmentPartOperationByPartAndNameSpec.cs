using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPartOperation;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.GarmentPartOperation.Create.Specifications;

public sealed class GarmentPartOperationByPartAndNameSpec : Specification<GarmentPartOperationEntity>
{
    public GarmentPartOperationByPartAndNameSpec(int garmentPartId, string name)
    {
        Query.Where(x => x.GarmentPartId == GarmentPartId.From(garmentPartId) && x.Name == name);
    }
}
