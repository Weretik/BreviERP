using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Production.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents.Production;

public sealed partial class ProductionPackageStore
{
    public async Task<Result<ProductionPackageDetail>> GetAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct)
    {
        var artifact = await AuthorizedAsync(quoteId, packageId, actor, ct);
        return artifact is null ? Result.NotFound() : await DetailResultAsync(artifact, quoteId, ct);
    }

    public async Task<Result<ProductionPackageContent>> ReadAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct)
    {
        var artifact = await AuthorizedAsync(quoteId, packageId, actor, ct);
        if (artifact is null) return Result.NotFound();
        var detail = await DetailResultAsync(artifact, quoteId, ct);
        return detail.IsSuccess ? Result.Success(new ProductionPackageContent(detail.Value, artifact.Bytes.ToArray()))
            : Result<ProductionPackageContent>.Error("production_package_content_missing_or_corrupt");
    }

    private async Task<ProductionPackageArtifact?> AuthorizedAsync(Guid quoteId, Guid packageId, CrmActor actor, CancellationToken ct)
    {
        if (quoteId == Guid.Empty || packageId == Guid.Empty || actor.UserId <= 0) return null;
        var quote = await db.Quotes.AsNoTracking().SingleOrDefaultAsync(x => x.PublicId == quoteId, ct);
        return !CanAccess(quote, actor) ? null : await db.Set<ProductionPackageArtifact>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.QuoteId == quote!.Id && x.PublicId == packageId, ct);
    }

    private async Task<Result<ProductionPackageDetail>> DetailResultAsync(ProductionPackageArtifact artifact, Guid quotePublicId, CancellationToken ct)
    {
        if (!artifact.IsIntact()) return Result.Error("production_package_content_missing_or_corrupt");
        var versionId = await db.QuoteVersions.AsNoTracking().Where(x => x.Id == artifact.QuoteVersionId && x.QuoteId == artifact.QuoteId)
            .Select(x => x.PublicId).SingleAsync(ct);
        return Result.Success(Detail(artifact, quotePublicId, versionId));
    }

    private static ProductionPackageDetail Detail(ProductionPackageArtifact artifact, Guid quotePublicId, Guid versionId) =>
        new(artifact.PublicId, quotePublicId, versionId, artifact.SchemaVersion, artifact.Sha256,
            artifact.Length, "application/zip", artifact.CreatedAt);
}
