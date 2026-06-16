using FabricEntity = Reference.Domain.Entities.Fabric;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.Fabric.Create.Specifications;

public sealed class FabricByIdOrNameSpec : Specification<FabricEntity>
{
    public FabricByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == FabricId.From(id) || x.Name == name);
    }
}
