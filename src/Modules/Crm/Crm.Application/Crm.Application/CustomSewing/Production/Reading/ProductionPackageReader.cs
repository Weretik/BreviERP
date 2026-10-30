using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Persistence;
namespace Crm.Application.CustomSewing.Production.Reading;
public sealed class ProductionPackageReader(IProductionPackageStore store)
{
    public Task<Result<ProductionPackageDetail>> GetAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct)
        => store.GetAsync(quoteId, packageId, actor, ct);
    public Task<Result<ProductionPackageContent>> ReadAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct)
        => store.ReadAsync(quoteId, packageId, actor, ct);
}
