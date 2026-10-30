using Ardalis.Result;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Projection;
namespace Crm.Application.CustomSewing.Production.Export;
public interface IProductionPackageArchiveBuilder
{
    Task<Result<byte[]>> BuildAsync(ProductionPackageManifest manifest,
        IReadOnlyList<ProductionCustomVersionSource> sources, CancellationToken ct);
}
