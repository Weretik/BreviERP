using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Create.Specifications;

public sealed class SupplierByIdOrNameSpec : Specification<SupplierEntity>
{
    public SupplierByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value == id || x.Name == name);
    }
}
