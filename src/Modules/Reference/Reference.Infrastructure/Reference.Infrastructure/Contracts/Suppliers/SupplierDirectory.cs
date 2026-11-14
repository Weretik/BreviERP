using Mediator;
using Reference.Contracts.Suppliers;
using Reference.Application.Features.Supplier.Queries.GetSuppliers;

namespace Reference.Infrastructure.Contracts.Suppliers;

internal sealed class SupplierDirectory(ISender sender) : ISupplierDirectory
{
    public async Task<IReadOnlyList<SupplierInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference suppliers could not be read.");
        return result.Value.Select(x => new SupplierInfo(x.Id, x.Name, x.IsActive, x.KeepinCrmId)).ToArray();
    }
}
