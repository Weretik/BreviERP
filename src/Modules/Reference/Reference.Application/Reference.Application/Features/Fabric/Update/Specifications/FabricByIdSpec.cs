using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Update.Specifications;

public sealed class FabricByIdSpec : Specification<FabricEntity>
{
    public FabricByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}
