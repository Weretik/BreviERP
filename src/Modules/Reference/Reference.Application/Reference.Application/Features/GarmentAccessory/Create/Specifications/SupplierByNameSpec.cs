using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.GarmentAccessory.Create.Specifications;

public sealed class SupplierByNameSpec : Specification<SupplierEntity>
{
    public SupplierByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
