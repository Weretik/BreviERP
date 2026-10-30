using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Domain.Customers.Identifiers;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Quotes.Idempotency;

internal sealed class QuoteMutationJournal(CrmDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static EncodedQuoteMutation Encode(string operation, Guid requestId, CrmActor actor, object intent)
    {
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(intent, JsonOptions)));
        var lockKey = BitConverter.ToInt64(SHA256.HashData(
            Encoding.UTF8.GetBytes($"quote:{actor.UserId}:{operation}:{requestId:N}")), 0);
        return new EncodedQuoteMutation(hash, lockKey);
    }

    public async Task<Result<QuoteDetail>?> ReadReplayAsync(
        string operation, Guid requestId, CrmActor actor, EncodedQuoteMutation encoded,
        CancellationToken cancellationToken)
    {
        var replay = await db.QuoteMutationRequests.AsNoTracking().SingleOrDefaultAsync(
            x => x.ActorId == actor.UserId && x.Operation == operation && x.RequestId == requestId,
            cancellationToken);
        if (replay is null) return null;
        var accessible = await db.Quotes.AsNoTracking().AnyAsync(x =>
            x.PublicId == replay.QuotePublicId &&
            (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)), cancellationToken);
        if (!accessible) return Result.NotFound();
        if (replay.IntentHash != encoded.Hash) return Result.Conflict("request_id_payload_conflict");
        return Result.Success(JsonSerializer.Deserialize<QuoteDetail>(replay.ResponseJson, JsonOptions)
            ?? throw new InvalidOperationException("Invalid quote mutation replay."));
    }

    public void Append(
        string operation, Guid requestId, CrmActor actor, EncodedQuoteMutation encoded, QuoteDetail response)
    {
        db.QuoteMutationRequests.Add(new QuoteMutationRecord
        {
            ActorId = actor.UserId,
            Operation = operation,
            RequestId = requestId,
            IntentHash = encoded.Hash,
            QuotePublicId = response.PublicId,
            ResponseJson = JsonSerializer.Serialize(response, JsonOptions),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }
}
