using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.Supplier.GetList.DTOs;
using Reference.Application.Features.Supplier.GetList.Specifications;
using SupplierEntity = Reference.Domain.Entities.Supplier;

namespace Reference.Application.Features.Supplier.GetList;

public sealed class GetSuppliersQueryHandler(IReferenceReadRepository<SupplierEntity> repository)
    : IQueryHandler<GetSuppliersQuery, Result<List<SupplierRowDTO>>>
{
    public async ValueTask<Result<List<SupplierRowDTO>>> Handle(
        GetSuppliersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.ListAsync(new GetSuppliersSpec(), cancellationToken);

        if (result is { Count: 0 })
            return Result.NotFound();

        return Result.Success(result);
    }
}
