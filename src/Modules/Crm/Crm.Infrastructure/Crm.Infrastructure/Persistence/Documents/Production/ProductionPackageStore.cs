using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Export;
using Crm.Application.CustomSewing.Production.Persistence;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Documents.Production;

public sealed partial class ProductionPackageStore(CrmDbContext db, TimeProvider? clock = null) : IProductionPackageStore
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;

    public async Task<Result<ProductionPackageDetail>> ExportAsync(ProductionPackageRequest request, CrmActor actor,
        Func<ProductionExportSource, CancellationToken, Task<Result<byte[]>>> build, CancellationToken ct)
    {
        if (actor.UserId <= 0 || request is null || request.RequestId == Guid.Empty || request.QuoteId == Guid.Empty ||
            request.VersionId == Guid.Empty || request.VariantIds is null || request.VariantIds.Count is < 1 or > 100 ||
            request.VariantIds.Any(x => x == Guid.Empty) || request.VariantIds.Distinct().Count() != request.VariantIds.Count)
            return Invalid("invalid_production_export");
        var selection = request.VariantIds.OrderBy(x => x).ToArray();
        var selectionHash = Hash(JsonSerializer.SerializeToUtf8Bytes(selection));
        var intentHash = Hash(JsonSerializer.SerializeToUtf8Bytes(new { request.QuoteId, request.VersionId, Selection = selection }));
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var committed = false;
        try
        {
            await LockAsync($"production-request:{actor.UserId}:{request.RequestId:N}", ct);
            var replay = await db.Set<ProductionPackageMutationRecord>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.ActorId == actor.UserId && x.RequestId == request.RequestId, ct);
            if (replay is not null)
            {
                var original = await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"Id\"={replay.QuoteId.Value} FOR UPDATE")
                    .AsNoTracking().SingleOrDefaultAsync(ct);
                if (!CanAccess(original, actor)) return Result.NotFound();
                if (replay.IntentHash != intentHash) return Result.Conflict("request_id_payload_conflict");
                var saved = await db.Set<ProductionPackageArtifact>().AsNoTracking().SingleAsync(x => x.Id == replay.PackageArtifactId, ct);
                return await DetailResultAsync(saved, original!.PublicId, ct);
            }

            var quote = await db.Quotes.FromSqlInterpolated($"SELECT * FROM crm.\"Quotes\" WHERE \"PublicId\"={request.QuoteId} FOR UPDATE")
                .AsNoTracking().SingleOrDefaultAsync(ct);
            if (!CanAccess(quote, actor)) return Result.NotFound();
            if (quote!.Archived) return Result.Conflict("production_quote_archived");
            var version = await db.QuoteVersions.AsNoTracking().AsSplitQuery()
                .Include(x => x.Items).ThenInclude(x => x.Variants).ThenInclude(x => x.Tiers)
                .SingleOrDefaultAsync(x => x.QuoteId == quote!.Id && x.PublicId == request.VersionId, ct);
            if (version is null) return Result.NotFound();
            var issue = await db.QuoteIssues.AsNoTracking().SingleOrDefaultAsync(x => x.QuoteId == quote!.Id && x.QuoteVersionId == version.Id, ct);
            if (issue is null) return Result.Conflict("production_issued_version_required");
            await LockAsync($"production-package:{quote!.Id.Value}:{version.Id}:{selectionHash}", ct);
            var artifact = await db.Set<ProductionPackageArtifact>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.QuoteId == quote.Id && x.QuoteVersionId == version.Id && x.SelectionHash == selectionHash, ct);
            if (artifact is null)
            {
                var source = await new ProductionPackageSourceReader(db).ReadAsync(quote.PublicId, issue.Number, version, selection, ct);
                if (!source.IsSuccess) return Failure(source);
                var result = await build(source.Value, ct);
                if (!result.IsSuccess) return Failure(result);
                if (result.Value is not { Length: > 0 and <= ProductionPackageArtifact.MaximumBytes } bytes ||
                    !bytes.AsSpan().StartsWith("PK\x03\x04"u8)) return Invalid("invalid_production_archive");
                artifact = new ProductionPackageArtifact { PublicId = Guid.NewGuid(), QuoteId = quote.Id,
                    QuoteVersionId = version.Id, SelectionHash = selectionHash, SchemaVersion = 1,
                    Bytes = bytes.ToArray(), Length = bytes.LongLength, Sha256 = Hash(bytes), CreatedAt = time.GetUtcNow() };
                db.Set<ProductionPackageArtifact>().Add(artifact);
                await db.SaveChangesAsync(ct);
            }
            if (!artifact.IsIntact()) return Result.Error("production_package_content_missing_or_corrupt");
            db.Set<ProductionPackageMutationRecord>().Add(new ProductionPackageMutationRecord {
                ActorId = actor.UserId, RequestId = request.RequestId, QuoteId = quote.Id,
                PackageArtifactId = artifact.Id, IntentHash = intentHash, CreatedAt = time.GetUtcNow() });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            committed = true;
            return Result.Success(Detail(artifact, quote.PublicId, version.PublicId));
        }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("production_export_conflict"); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result.Conflict("production_export_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }

    private Task LockAsync(string key, CancellationToken ct) => db.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT pg_advisory_xact_lock({BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(key)), 0)})", ct);
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static bool CanAccess(Quote? quote, CrmActor actor) => quote is not null && actor.UserId > 0 &&
        (actor.IsAdmin || quote.OwnerId == new CrmUserId(actor.UserId));
    private static Result<ProductionPackageDetail> Invalid(string code) =>
        Result<ProductionPackageDetail>.Invalid(new ValidationError("productionPackage", code));
    private static Result<ProductionPackageDetail> Failure<T>(Result<T> result) => result.Status switch {
        ResultStatus.Invalid => Result<ProductionPackageDetail>.Invalid(result.ValidationErrors),
        ResultStatus.NotFound => Result<ProductionPackageDetail>.NotFound(),
        ResultStatus.Forbidden => Result<ProductionPackageDetail>.Forbidden(),
        ResultStatus.Conflict => Result<ProductionPackageDetail>.Conflict(result.Errors.ToArray()),
        _ => Result<ProductionPackageDetail>.Error(string.Join(',', result.Errors)) };
}
