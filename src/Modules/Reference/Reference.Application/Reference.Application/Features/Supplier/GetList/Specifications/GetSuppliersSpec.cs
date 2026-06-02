using Reference.Application.Features.Supplier.GetList.DTOs;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.GetList.Specifications;

public sealed class GetSuppliersSpec : Specification<SupplierEntity, SupplierRowDTO>
{
    public GetSuppliersSpec()
    {
        Query.AsNoTracking()
            .OrderBy(x => x.Id.Value)
            .Select(x => new SupplierRowDTO(x.Id.Value, x.Name, x.Link));
    }
}
