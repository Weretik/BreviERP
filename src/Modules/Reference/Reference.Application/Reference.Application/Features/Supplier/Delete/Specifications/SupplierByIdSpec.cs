using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.Delete.Specifications;

public sealed class SupplierByIdSpec : Specification<SupplierEntity>
{
    public SupplierByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}
