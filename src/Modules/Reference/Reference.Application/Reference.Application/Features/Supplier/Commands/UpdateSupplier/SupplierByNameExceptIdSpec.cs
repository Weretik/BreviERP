using BuildingBlocks.Domain.Identifiers.ReferenceData;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.Supplier.Commands.UpdateSupplier;

public sealed class SupplierByNameExceptIdSpec : Specification<SupplierEntity>
{
    public SupplierByNameExceptIdSpec(int id, string name)
    {
        Query.Where(x => x.Id != new SupplierId(id) && x.Name == name);
    }
}
