using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Persistence;
using Crm.Application.CustomSewing.Production.Projection;
namespace Crm.Application.CustomSewing.Production.Export;
public sealed class ProductionPackageExporter(IProductionPackageStore store, IProductionPackageArchiveBuilder archives)
{
    public Task<Result<ProductionPackageDetail>> ExportAsync(ProductionPackageRequest request, CrmActor actor, CancellationToken ct)
    {
        if (actor.UserId <= 0 || request.QuoteId == Guid.Empty || request.VersionId == Guid.Empty ||
            request.RequestId == Guid.Empty || request.VariantIds is null || request.VariantIds.Count is < 1 or > 100 ||
            request.VariantIds.Any(id => id == Guid.Empty) || request.VariantIds.Distinct().Count() != request.VariantIds.Count)
            return Task.FromResult(Result<ProductionPackageDetail>.Invalid(new ValidationError("request", "invalid_production_export")));
        return store.ExportAsync(request, actor, async (source, token) =>
        {
            var projection = ProductionPackageProjectionBuilder.Build(source.QuotePublicId,
                source.QuoteNumber, source.Version, request.VariantIds, source.CustomVersions);
            if (projection.Status == ResultStatus.Invalid) return Result<byte[]>.Invalid(projection.ValidationErrors);
            if (!projection.IsSuccess) return Result<byte[]>.Conflict(projection.Errors.ToArray());
            return await archives.BuildAsync(projection.Value, source.CustomVersions, token);
        }, ct);
    }
}
