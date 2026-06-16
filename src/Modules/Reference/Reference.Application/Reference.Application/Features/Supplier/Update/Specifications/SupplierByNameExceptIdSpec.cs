using SupplierEntity = Reference.Domain.Entities.Supplier;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.Supplier.Update.Specifications;

public sealed class SupplierByNameExceptIdSpec : Specification<SupplierEntity>
{
    public SupplierByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != SupplierId.From(id) && x.Name == name);
    }
}
