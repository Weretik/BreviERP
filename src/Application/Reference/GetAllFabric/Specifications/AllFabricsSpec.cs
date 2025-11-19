// Licensed to KedrStore Development Team under MIT License.

using Domain.Reference.Entities;

namespace Application.Reference.GetAllFabric;

public sealed class AllFabricsSpec : Specification<Fabric>
{
    public AllFabricsSpec()
    {
        Query.AsNoTracking();
    }
}
