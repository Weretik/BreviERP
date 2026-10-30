using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Domain.Documents.EmailTemplates;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents.EmailTemplates;

public sealed partial class EmailTemplateStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private async Task<Result<T>> ExecuteAsync<T>(string operation, CrmActor actor, Guid requestId,
        object intent, Func<CancellationToken, Task<Result<T>>> work, CancellationToken ct)
    {
        if (!actor.IsAdmin || actor.UserId <= 0) return Result<T>.Forbidden();
        if (requestId == Guid.Empty) return Result<T>.Invalid(new ValidationError("requestId", "invalid_request"));
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(intent, Json)));
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var committed = false;
        try
        {
            // Low-volume company settings: serialize number allocation, active pointer and receipts together.
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(4849701693292229)", ct);
            var old = await db.Set<EmailTemplateMutationRecord>().AsNoTracking().SingleOrDefaultAsync(x =>
                x.ActorId == actor.UserId && x.Operation == operation && x.RequestId == requestId, ct);
            if (old is not null)
                return old.IntentHash == hash ? Result.Success(JsonSerializer.Deserialize<T>(old.ResponseJson, Json)
                    ?? throw new InvalidDataException("Email template replay is invalid."))
                    : Result<T>.Conflict("request_id_payload_conflict");
            var result = await work(ct);
            if (!result.IsSuccess) return result;
            db.Set<EmailTemplateMutationRecord>().Add(new EmailTemplateMutationRecord {
                ActorId = actor.UserId, Operation = operation, RequestId = requestId, IntentHash = hash,
                ResponseJson = JsonSerializer.Serialize(result.Value, Json), CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            committed = true;
            return result;
        }
        catch (EmailTemplateValidationException e)
        {
            return e.Message == "revision_conflict" ? Result<T>.Conflict(e.Message)
                : Result<T>.Invalid(new ValidationError("template", e.Message));
        }
        catch (DbUpdateConcurrencyException) { return Result<T>.Conflict("revision_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }
}
