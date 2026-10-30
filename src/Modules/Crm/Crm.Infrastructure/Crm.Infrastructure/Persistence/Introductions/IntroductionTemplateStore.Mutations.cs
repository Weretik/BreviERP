using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Introductions;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Introductions;

public sealed partial class IntroductionTemplateStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private async Task<Result<IntroductionTemplateDetail>> ExecuteAsync(string operation, Guid requestId,
        CrmActor actor, object intent,
        Func<CancellationToken, Task<(Result<IntroductionTemplateDetail> Result, IntroductionTemplatePayload? Payload)>> work,
        CancellationToken ct)
    {
        if (actor.UserId <= 0) return Result<IntroductionTemplateDetail>.Forbidden();
        if (requestId == Guid.Empty) return Result<IntroductionTemplateDetail>.Invalid(new ValidationError("requestId", "invalid_request"));
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(intent, Json)));
        var lockKey = BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"introduction-template:{actor.UserId}:{operation}:{requestId:N}")), 0);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", ct);
            var replay = await db.Set<IntroductionTemplateMutationRecord>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.ActorId == actor.UserId && x.Operation == operation &&
                    x.RequestId == requestId, ct);
            if (replay is not null)
            {
                if (!await Accessible(actor).AnyAsync(x => x.PublicId == replay.TemplatePublicId, ct))
                    return Result<IntroductionTemplateDetail>.NotFound();
                if (replay.IntentHash != hash)
                    return Result<IntroductionTemplateDetail>.Conflict("request_id_payload_conflict");
                var saved = JsonSerializer.Deserialize<IntroductionTemplateReplay>(replay.ResponseJson, Json)
                    ?? throw new InvalidDataException("Introduction template replay is missing.");
                return Result.Success(saved.ToDetail());
            }
            var (result, payload) = await work(ct);
            if (!result.IsSuccess) return result;
            if (payload is null) throw new InvalidOperationException("Successful template mutation has no payload.");
            db.Set<IntroductionTemplateMutationRecord>().Add(new IntroductionTemplateMutationRecord
            {
                ActorId = actor.UserId, Operation = operation, RequestId = requestId,
                TemplatePublicId = result.Value.TemplatePublicId, IntentHash = hash,
                ResponseJson = JsonSerializer.Serialize(IntroductionTemplateReplay.FromDetail(result.Value, payload), Json),
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            committed = true;
            return result;
        }
        catch (IntroductionValidationException exception)
        {
            return exception.Message is "revision_conflict" or "template_archived"
                ? Result<IntroductionTemplateDetail>.Conflict(exception.Message)
                : Result<IntroductionTemplateDetail>.Invalid(new ValidationError("template", exception.Message));
        }
        catch (DbUpdateConcurrencyException) { return Result<IntroductionTemplateDetail>.Conflict("revision_conflict"); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result<IntroductionTemplateDetail>.Conflict("template_unique_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }

}
