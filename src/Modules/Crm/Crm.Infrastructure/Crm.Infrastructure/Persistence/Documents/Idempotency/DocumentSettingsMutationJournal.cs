using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents;

internal sealed class DocumentSettingsMutationJournal(CrmDbContext db)
{
    public static string Hash<T>(T value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));

    public async Task<Result<T>?> ReplayAsync<T>(string operation, Guid requestId, int actorId, string hash, CancellationToken ct)
    {
        var record = await db.Set<DocumentSettingsMutationRecord>().AsNoTracking().SingleOrDefaultAsync(
            x => x.Operation == operation && x.ActorId == actorId && x.RequestId == requestId, ct);
        if (record is null) return null;
        if (record.IntentHash != hash) return Result.Conflict("idempotency_conflict");
        return Result.Success(JsonSerializer.Deserialize<T>(record.ResponseJson)
            ?? throw new InvalidOperationException("Stored settings response is invalid."));
    }

    public void Append<T>(string operation, Guid requestId, int actorId, string hash, T response)
        => db.Add(new DocumentSettingsMutationRecord
        {
            Operation = operation, RequestId = requestId, ActorId = actorId,
            IntentHash = hash, ResponseJson = JsonSerializer.Serialize(response)
        });
}
