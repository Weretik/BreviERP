using GarmentPartEntity = Reference.Domain.GarmentPartOperations.Entities.GarmentPart;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.GarmentPart.Update.Specifications;

public sealed class GarmentPartByIdSpec : Specification<GarmentPartEntity>
{
    public GarmentPartByIdSpec(int id)
    {
        Query.Where(x => x.Id == GarmentPartId.From(id));
    }
}
