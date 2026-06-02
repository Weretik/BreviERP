using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Update.Specifications;

public sealed class SupplierByNameExceptIdSpec : Specification<SupplierEntity>
{
    public SupplierByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id.Value != id && x.Name == name);
    }
}
