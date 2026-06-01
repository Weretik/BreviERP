namespace Reference.Application.Features.Fabric.GetList.Specifications;

public sealed class AllFabricsSpec : Specification<Domain.Entities.Fabric>
{
    public AllFabricsSpec()
    {
        Query.AsNoTracking();
    }
}
