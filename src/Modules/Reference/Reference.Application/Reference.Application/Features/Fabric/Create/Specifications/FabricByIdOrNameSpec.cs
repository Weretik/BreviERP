using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Create.Specifications;

public sealed class FabricByIdOrNameSpec : Specification<FabricEntity>
{
    public FabricByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value == id || x.Name == name);
    }
}
