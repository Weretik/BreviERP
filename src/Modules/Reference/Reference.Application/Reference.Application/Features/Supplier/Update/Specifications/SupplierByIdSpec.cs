using SupplierEntity = Reference.Domain.Entities.Supplier;
using Reference.Domain.ValueObjects;

namespace Reference.Application.Features.Supplier.Update.Specifications;

public sealed class SupplierByIdSpec : Specification<SupplierEntity>
{
    public SupplierByIdSpec(int id)
    {
        Query.Where(x => x.Id == SupplierId.From(id));
    }
}
