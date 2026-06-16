using SupplierEntity = Reference.Domain.Entities.Supplier;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.Supplier.Create.Specifications;

public sealed class SupplierByIdOrNameSpec : Specification<SupplierEntity>
{
    public SupplierByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == SupplierId.From(id) || x.Name == name);
    }
}
