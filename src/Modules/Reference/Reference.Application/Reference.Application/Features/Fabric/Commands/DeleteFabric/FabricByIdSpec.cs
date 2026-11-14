using FabricEntity = Reference.Domain.Fabrics.Fabric;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Reference.Domain.Fabrics;

namespace Reference.Application.Features.Fabric.Commands.DeleteFabric;

public sealed class FabricByIdSpec : Specification<FabricEntity>
{
    public FabricByIdSpec(int id)
    {
        Query.Where(x => x.Id == FabricId.From(id));
    }
}
