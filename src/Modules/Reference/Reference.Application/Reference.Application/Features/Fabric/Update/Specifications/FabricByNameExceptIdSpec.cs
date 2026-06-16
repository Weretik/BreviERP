using FabricEntity = Reference.Domain.Entities.Fabric;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.Fabric.Update.Specifications;

public sealed class FabricByNameExceptIdSpec : Specification<FabricEntity>
{
    public FabricByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != FabricId.From(id) && x.Name == name);
    }
}
