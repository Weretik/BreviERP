using FabricEntity = Reference.Domain.GarmentAccessories.Entities.Fabric;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.Fabric.Create.Specifications;

public sealed class FabricByIdOrNameSpec : Specification<FabricEntity>
{
    public FabricByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == FabricId.From(id) || x.Name == name);
    }
}
