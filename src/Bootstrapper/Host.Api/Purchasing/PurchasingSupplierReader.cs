using Mediator;
using Purchasing.Application.Abstractions;
using Reference.Application.Features.Supplier.GetList;

namespace Host.Api.Purchasing;

/// <summary>«Постачальники» (Reference) for the manual supplier link of purchase invoices.</summary>
internal sealed class PurchasingSupplierReader(ISender sender) : IPurchasingSupplierReader
{
    public async Task<IReadOnlyList<PurchasingSupplier>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference suppliers could not be read.");
        return result.Value.Select(x => new PurchasingSupplier(x.Id, x.Name, x.IsActive)).ToArray();
    }
}
