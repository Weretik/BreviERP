using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
using Crm.Infrastructure.Persistence.Templates.Writing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Templates.Idempotency;

internal sealed class TemplateMutationJournal(CrmDbContext db)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    internal static EncodedTemplateMutation Encode(string operation, Guid requestId, CrmActor actor, object intent)
        => new(Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(intent, Json))),
            BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes($"template:{actor.UserId}:{operation}:{requestId:N}")), 0));

    internal async Task<Result<TemplateDetail>?> ReadReplayAsync(string operation, Guid requestId, CrmActor actor,
        EncodedTemplateMutation encoded, CancellationToken ct)
    {
        var replay = await db.Set<TemplateMutationRecord>().AsNoTracking().SingleOrDefaultAsync(x =>
            x.ActorId == actor.UserId && x.Operation == operation && x.RequestId == requestId, ct);
        if (replay is null) return null;
        if (!await db.Set<CustomTemplate>().AnyAsync(x => x.PublicId == replay.TemplatePublicId
            && (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)), ct)) return Result.NotFound();
        if (encoded.Hash != replay.IntentHash) return Result.Conflict("request_id_payload_conflict");
        return Result.Success(JsonSerializer.Deserialize<TemplateDetail>(replay.ResponseJson, Json)
            ?? throw new InvalidOperationException("Invalid template replay."));
    }

    internal void Append(string operation, Guid requestId, CrmActor actor, EncodedTemplateMutation encoded,
        TemplateDetail response)
        => db.Set<TemplateMutationRecord>().Add(new TemplateMutationRecord
        {
            ActorId = actor.UserId, Operation = operation, RequestId = requestId,
            TemplatePublicId = response.PublicId, IntentHash = encoded.Hash,
            ResponseJson = JsonSerializer.Serialize(response, Json), CreatedAt = DateTimeOffset.UtcNow
        });
}
