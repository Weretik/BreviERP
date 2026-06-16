using Reference.Domain.ValueObjects;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.GarmentPart.Create.Specifications;

public sealed class SupplierByIdSpec : Specification<SupplierEntity>
{
    public SupplierByIdSpec(int id)
    {
        Query.Where(x => x.Id == SupplierId.From(id));
    }
}
