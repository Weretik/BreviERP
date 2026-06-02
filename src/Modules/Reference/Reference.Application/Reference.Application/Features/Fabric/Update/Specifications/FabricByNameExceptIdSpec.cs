using FabricEntity = Reference.Domain.Entities.Fabric;

namespace Reference.Application.Features.Fabric.Update.Specifications;

public sealed class FabricByNameExceptIdSpec : Specification<FabricEntity>
{
    public FabricByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value != id && x.Name == name);
    }
}
