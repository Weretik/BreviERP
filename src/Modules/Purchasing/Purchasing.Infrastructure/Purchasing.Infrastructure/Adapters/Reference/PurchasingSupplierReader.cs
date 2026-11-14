using Purchasing.Application.Abstractions;

using Reference.Contracts.Suppliers;

namespace Purchasing.Infrastructure.Adapters.Reference;

/// <summary>«Постачальники» (Reference) for the manual supplier link of purchase invoices.</summary>
internal sealed class PurchasingSupplierReader(ISupplierDirectory suppliers) : IPurchasingSupplierReader
{
    public async Task<IReadOnlyList<PurchasingSupplier>> ListAsync(CancellationToken cancellationToken) =>
        (await suppliers.ListAsync(cancellationToken)).Select(x => new PurchasingSupplier(x.Id, x.Name, x.IsActive))
        .ToArray();
}
