using FabricEntity = Reference.Domain.GarmentAccessories.Fabric;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.Fabric.Create.Specifications;

public sealed class FabricByIdSpec : Specification<FabricEntity>
{
    public FabricByIdSpec(int id)
    {
        Query.Where(x => x.Id == FabricId.From(id));
    }
}
