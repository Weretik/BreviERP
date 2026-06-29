using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Create.Specifications;

public sealed class SupplierByNameSpec : Specification<SupplierEntity>
{
    public SupplierByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
