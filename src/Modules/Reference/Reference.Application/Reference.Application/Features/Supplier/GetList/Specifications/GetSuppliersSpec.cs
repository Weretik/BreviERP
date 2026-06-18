using Reference.Application.Features.Supplier.GetList.DTOs;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.GetList.Specifications;

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
                x.Notes));
    }
}
