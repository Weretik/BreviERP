using SupplierEntity = Reference.Domain.Suppliers.Supplier;

namespace Reference.Application.Features.Fabric.Update.Specifications;

public sealed class SupplierByNameSpec : Specification<SupplierEntity>
{
    public SupplierByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
