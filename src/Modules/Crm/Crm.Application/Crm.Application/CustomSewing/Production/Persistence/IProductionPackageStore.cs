using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Export;
namespace Crm.Application.CustomSewing.Production.Persistence;
public interface IProductionPackageStore
{
    Task<Result<ProductionPackageDetail>> ExportAsync(ProductionPackageRequest request, CrmActor actor,
        Func<ProductionExportSource, CancellationToken, Task<Result<byte[]>>> build, CancellationToken ct);
    Task<Result<ProductionPackageDetail>> GetAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct);
    Task<Result<ProductionPackageContent>> ReadAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct);
}
