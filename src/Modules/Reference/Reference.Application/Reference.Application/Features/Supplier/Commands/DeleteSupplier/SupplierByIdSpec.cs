using BuildingBlocks.Domain.Identifiers.ReferenceData;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.Supplier.Commands.DeleteSupplier;

public sealed class SupplierByIdSpec : Specification<SupplierEntity>
{
    public SupplierByIdSpec(int id)
    {
        Query.Where(x => x.Id == new SupplierId(id));
    }
}
