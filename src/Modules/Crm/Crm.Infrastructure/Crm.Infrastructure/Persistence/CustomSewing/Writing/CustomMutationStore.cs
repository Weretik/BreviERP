using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Infrastructure.Persistence.CustomSewing.Idempotency;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.Persistence;
using Crm.Domain.CustomSewing;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.CustomSewing.Writing;

public sealed class CustomMutationStore(CrmDbContext db) : ICustomMutationStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task<Result<CustomDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomWriteSession, CancellationToken, Task<Result<CustomDetail>>> work, CancellationToken ct)
    {
        if (requestId == Guid.Empty || actor.UserId <= 0) return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        var encoded = CustomMutationJournal.Encode(operation, requestId, actor, intent);
        var journal = new CustomMutationJournal(db);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({encoded.LockKey})", ct);
            var replay = await journal.ReadReplayAsync(operation, requestId, actor, encoded, ct);
            if (replay is not null) return replay;
            var result = await work(new CustomWriteSession(db, actor), ct);
            if (!result.IsSuccess) return result;
            journal.Append(operation, requestId, actor, encoded, result.Value);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            committed = true;
            return result;
        }
        catch (CustomSewingRevisionException) { return Result.Conflict("revision_conflict"); }
        catch (CustomSewingValidationException ex) { return Result.Invalid(new ValidationError("configuration", ex.Message)); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("revision_conflict"); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result.Conflict("custom_unique_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }
}
