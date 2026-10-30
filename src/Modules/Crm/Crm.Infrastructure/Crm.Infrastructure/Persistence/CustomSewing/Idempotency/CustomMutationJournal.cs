using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.CustomSewing.Idempotency;

internal sealed class CustomMutationJournal(CrmDbContext db)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    internal static EncodedCustomMutation Encode(string operation, Guid requestId, CrmActor actor, object intent)
        => new(Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(intent, Json))),
            BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes($"custom:{actor.UserId}:{operation}:{requestId:N}")), 0));

    internal async Task<Result<CustomDetail>?> ReadReplayAsync(string operation, Guid requestId, CrmActor actor,
        EncodedCustomMutation encoded, CancellationToken ct)
    {
        var replay = await db.CustomMutations.AsNoTracking().SingleOrDefaultAsync(x => x.ActorId == actor.UserId
            && x.Operation == operation && x.RequestId == requestId, ct);
        if (replay is null) return null;
        if (!await db.CustomDrafts.Where(CustomAccess.For(actor)).AnyAsync(x => x.PublicId == replay.DraftPublicId, ct))
            return Result.NotFound();
        if (replay.IntentHash != encoded.Hash) return Result.Conflict("request_id_payload_conflict");
        return Result.Success(JsonSerializer.Deserialize<CustomDetail>(replay.ResponseJson, Json)
            ?? throw new InvalidOperationException("Invalid custom replay."));
    }

    internal void Append(string operation, Guid requestId, CrmActor actor, EncodedCustomMutation encoded, CustomDetail response)
        => db.CustomMutations.Add(new CustomMutationRecord { ActorId = actor.UserId, Operation = operation, RequestId = requestId,
            IntentHash = encoded.Hash, DraftPublicId = response.PublicId, ResponseJson = JsonSerializer.Serialize(response, Json),
            CreatedAt = DateTimeOffset.UtcNow });
}
