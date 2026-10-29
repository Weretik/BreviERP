using Ardalis.Result;
using Crm.Application.Documents.Artifacts;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Documents.Artifacts;

public sealed class QuotePdfArtifactStore(CrmDbContext db, TimeProvider? clock = null)
    : IQuotePdfArtifactStore
{
    private static readonly TimeSpan WorkerLease = TimeSpan.FromMinutes(5);
    private readonly TimeProvider time = clock ?? TimeProvider.System;

    public async Task<Result<QuotePdfArtifactMetadata>> QueueAsync(QuoteId quoteId, long quoteVersionId,
        IssuerSettingsVersionId issuerVersionId, DocumentTemplateVersionId templateVersionId,
        string renderKey, CancellationToken cancellationToken)
    {
        QuotePdfArtifact artifact;
        try { artifact = QuotePdfArtifact.Queue(quoteId, quoteVersionId, issuerVersionId,
            templateVersionId, renderKey, time.GetUtcNow()); }
        catch (ArgumentException) { return Result.Invalid(new ValidationError("artifact", "invalid_artifact_identity")); }

        db.Set<QuotePdfArtifact>().Add(artifact);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: { } name } &&
            name.StartsWith("IX_QuotePdfArtifacts_QuoteId_QuoteVersionId_", StringComparison.Ordinal))
        {
            db.Entry(artifact).State = EntityState.Detached;
            var existing = await db.Set<QuotePdfArtifact>().AsNoTracking()
                .Where(x => x.QuoteId == quoteId && x.QuoteVersionId == quoteVersionId &&
                    x.IssuerVersionId == issuerVersionId && x.TemplateVersionId == templateVersionId &&
                    x.RenderKey == renderKey)
                .Select(x => new QuotePdfArtifactMetadata(x.PublicId, x.QuoteId,
                    x.QuoteVersionId, x.IssuerVersionId, x.TemplateVersionId, x.RenderKey,
                    x.Status, x.Attempt, x.Sha256, x.Length, x.ErrorCode,
                    x.CreatedAt, x.UpdatedAt, x.GeneratedAt))
                .SingleAsync(cancellationToken);
            return Result.Success(existing);
        }
        return Result.Success(artifact.Metadata());
    }

    public async Task<QuotePdfArtifactClaim?> ClaimNextAsync(CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // Serializes worker claims, including stale-lease recovery, across API instances.
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(2048230274)", cancellationToken);
        var artifact = await db.Set<QuotePdfArtifact>()
            .Where(x => x.Status == "queued" ||
                (x.Status == "processing" && x.LeaseUntil <= now))
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (artifact is null) return null;
        var token = artifact.Claim(now, WorkerLease);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new QuotePdfArtifactClaim(artifact.Metadata(), token);
    }

    public async Task<Result<QuotePdfArtifactMetadata>> CompleteAsync(Guid artifactId, Guid leaseToken,
        byte[] pdfBytes, CancellationToken cancellationToken)
    {
        var artifact = await db.Set<QuotePdfArtifact>()
            .SingleOrDefaultAsync(x => x.PublicId == artifactId, cancellationToken);
        if (artifact is null) return Result.NotFound();
        try { artifact.Complete(leaseToken, pdfBytes, time.GetUtcNow()); }
        catch (ArgumentException) { return Result.Invalid(new ValidationError("pdf", "invalid_pdf_bytes")); }
        catch (InvalidOperationException) { return Result.Conflict("artifact_lease_conflict"); }
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("artifact_lease_conflict"); }
        return Result.Success(artifact.Metadata());
    }

    public async Task<Result<QuotePdfArtifactMetadata>> FailAsync(Guid artifactId, Guid leaseToken,
        string errorCode, CancellationToken cancellationToken)
    {
        var artifact = await db.Set<QuotePdfArtifact>()
            .SingleOrDefaultAsync(x => x.PublicId == artifactId, cancellationToken);
        if (artifact is null) return Result.NotFound();
        try { artifact.Fail(leaseToken, errorCode, time.GetUtcNow()); }
        catch (ArgumentException) { return Result.Invalid(new ValidationError("errorCode", "invalid_error_code")); }
        catch (InvalidOperationException) { return Result.Conflict("artifact_lease_conflict"); }
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("artifact_lease_conflict"); }
        return Result.Success(artifact.Metadata());
    }

    public async Task<Result<QuotePdfArtifactMetadata>> RetryAsync(Guid artifactId,
        CancellationToken cancellationToken)
    {
        var artifact = await db.Set<QuotePdfArtifact>()
            .SingleOrDefaultAsync(x => x.PublicId == artifactId, cancellationToken);
        if (artifact is null) return Result.NotFound();
        try { artifact.Retry(time.GetUtcNow()); }
        catch (InvalidOperationException) { return Result.Conflict("artifact_not_retryable"); }
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("artifact_retry_conflict"); }
        return Result.Success(artifact.Metadata());
    }

    public Task<QuotePdfArtifactMetadata?> FindAsync(Guid artifactId, CancellationToken cancellationToken)
        => db.Set<QuotePdfArtifact>().AsNoTracking().Where(x => x.PublicId == artifactId)
            .Select(x => new QuotePdfArtifactMetadata(x.PublicId, x.QuoteId,
                x.QuoteVersionId, x.IssuerVersionId, x.TemplateVersionId, x.RenderKey,
                x.Status, x.Attempt, x.Sha256, x.Length, x.ErrorCode,
                x.CreatedAt, x.UpdatedAt, x.GeneratedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<Result<QuotePdfArtifactContent>> ReadContentAsync(Guid artifactId,
        CancellationToken cancellationToken)
    {
        var artifact = await db.Set<QuotePdfArtifact>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.PublicId == artifactId, cancellationToken);
        if (artifact is null) return Result.NotFound();
        if (artifact.Status != "succeeded") return Result<QuotePdfArtifactContent>.Conflict("artifact_not_ready");
        if (!artifact.HasIntactContent()) return Result.Error("artifact_content_missing_or_corrupt");
        return Result.Success(new QuotePdfArtifactContent(artifact.Metadata(), artifact.PdfBytes!));
    }
}
