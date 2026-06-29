using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.Supplier.Update.Specifications;

public sealed class SupplierByIdSpec : Specification<SupplierEntity>
{
    public SupplierByIdSpec(int id)
    {
        Query.Where(x => x.Id == SupplierId.From(id));
    }
}
