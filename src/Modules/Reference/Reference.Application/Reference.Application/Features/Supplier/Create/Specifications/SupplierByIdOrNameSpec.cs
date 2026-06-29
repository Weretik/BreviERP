using SupplierEntity = Reference.Domain.Suppliers.Entities.Supplier;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Application.Features.Supplier.Create.Specifications;

public sealed class SupplierByIdOrNameSpec : Specification<SupplierEntity>
{
    public SupplierByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == SupplierId.From(id) || x.Name == name);
    }
}
