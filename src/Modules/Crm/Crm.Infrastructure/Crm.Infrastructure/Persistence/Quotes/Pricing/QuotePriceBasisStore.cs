using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Pricing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Quotes.Pricing;

public sealed class QuotePriceBasisStore(CrmDbContext db) : IQuotePriceBasisStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<QuotePricePreview>> ExecuteAsync(
        Guid requestId, Guid? quoteId, CrmActor actor, QuotePriceIntent intent,
        Func<CancellationToken, Task<Result<QuotePricePreview>>> work,
        CancellationToken cancellationToken)
    {
        if (requestId == Guid.Empty || actor.UserId <= 0)
            return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        var hash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(new { quoteId, intent }, JsonOptions)));
        var lockKey = BitConverter.ToInt64(SHA256.HashData(
            Encoding.UTF8.GetBytes($"quote-price:{actor.UserId}:{requestId:N}")), 0);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock({lockKey})", cancellationToken);
            var old = await db.Set<QuotePriceBasisRecord>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.ActorId == actor.UserId && x.RequestId == requestId, cancellationToken);
            if (old is not null)
                return old.IntentHash == hash
                    ? Result.Success(Deserialize(old.SnapshotJson))
                    : Result.Conflict("request_id_payload_conflict");

            var result = await work(cancellationToken);
            if (!result.IsSuccess) return result;
            var preview = result.Value;
            db.Set<QuotePriceBasisRecord>().Add(new QuotePriceBasisRecord
            {
                PublicId = preview.BasisToken,
                RequestId = requestId,
                ActorId = actor.UserId,
                QuotePublicId = quoteId,
                IntentHash = hash,
                SnapshotJson = JsonSerializer.Serialize(preview, JsonOptions),
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = preview.ExpiresAt
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committed = true;
            return result;
        }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }

    public async Task<QuotePricePreview?> FindAsync(
        Guid token, Guid? quoteId, CrmActor actor, CancellationToken cancellationToken)
    {
        if (token == Guid.Empty || actor.UserId <= 0) return null;
        var row = await db.Set<QuotePriceBasisRecord>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.PublicId == token && x.ActorId == actor.UserId &&
                x.QuotePublicId == quoteId, cancellationToken);
        return row is not null && row.ExpiresAt > DateTimeOffset.UtcNow
            ? Deserialize(row.SnapshotJson)
            : null;
    }

    private static QuotePricePreview Deserialize(string json)
        => JsonSerializer.Deserialize<QuotePricePreview>(json, JsonOptions)
            ?? throw new InvalidOperationException("Invalid stored quote price basis.");
}
