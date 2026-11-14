using SupplierEntity = Reference.Domain.Suppliers.Supplier;

namespace Reference.Application.Features.Supplier.Queries.GetSuppliers;

public sealed class GetSuppliersSpec : Specification<SupplierEntity, SupplierRowDTO>
{
    public GetSuppliersSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new SupplierRowDTO(
                x.Id.Value,
                x.Name,
                x.Link,
                x.ContactPerson,
                x.PhoneNumber,
                x.Notes,
                x.Email,
                x.PurchaseCategories,
                x.Tags,
                x.IsActive,
                x.KeepinCrmId,
                x.KeepinSyncedAt));
    }
}
