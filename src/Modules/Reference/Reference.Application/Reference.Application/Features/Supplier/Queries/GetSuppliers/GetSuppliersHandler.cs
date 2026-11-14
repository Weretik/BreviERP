using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.Supplier.Queries.GetSuppliers;

public sealed class GetSuppliersHandler(IReferenceReadRepository<SupplierEntity> repository)
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
