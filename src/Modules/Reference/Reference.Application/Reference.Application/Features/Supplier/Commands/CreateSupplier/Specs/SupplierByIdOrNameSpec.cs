using BuildingBlocks.Domain.Identifiers.ReferenceData;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;

namespace Reference.Application.Features.Supplier.Commands.CreateSupplier.Specs;

public sealed class SupplierByIdOrNameSpec : Specification<SupplierEntity>
{
    public SupplierByIdOrNameSpec(int id, string name)
    {
        Query.Where(x => x.Id == new SupplierId(id) || x.Name == name);
    }
}
