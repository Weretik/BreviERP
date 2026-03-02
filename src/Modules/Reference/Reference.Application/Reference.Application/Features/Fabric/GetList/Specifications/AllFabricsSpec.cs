using Reference.Domain.Entities;

namespace Reference.Application.GetAllFabric.Specifications;

public sealed class AllFabricsSpec : Specification<Fabric>
{
    public AllFabricsSpec()
    {
        Query.AsNoTracking();
    }
}
