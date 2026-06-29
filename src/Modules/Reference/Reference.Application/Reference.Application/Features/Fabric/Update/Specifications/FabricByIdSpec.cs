using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.Fabric.Update.Specifications;

public sealed class FabricByIdSpec : Specification<FabricEntity>
{
    public FabricByIdSpec(int id)
    {
        Query.Where(x => x.Id == FabricId.From(id));
    }
}
