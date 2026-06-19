using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Create.Specifications;

public sealed class FabricByNameSpec : Specification<FabricEntity>
{
    public FabricByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
