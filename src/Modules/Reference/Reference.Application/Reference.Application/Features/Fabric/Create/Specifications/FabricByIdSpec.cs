using FabricEntity = Reference.Domain.Entities.Fabric;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.Fabric.Create.Specifications;

public sealed class FabricByIdSpec : Specification<FabricEntity>
{
    public FabricByIdSpec(int id)
    {
        Query.Where(x => x.Id == FabricId.From(id));
    }
}
