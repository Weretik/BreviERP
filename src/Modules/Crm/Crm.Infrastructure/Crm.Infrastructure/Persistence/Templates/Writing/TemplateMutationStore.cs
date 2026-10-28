using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.CustomSewing;
using Crm.Infrastructure.Persistence.Templates.Idempotency;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Templates.Writing;

public sealed class TemplateMutationStore(CrmDbContext db) : ITemplateMutationStore
{
    public async Task<Result<TemplateDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ITemplateWriteSession, CancellationToken, Task<Result<TemplateDetail>>> work, CancellationToken ct)
    {
        if (requestId == Guid.Empty || actor.UserId <= 0) return Result.Invalid(new ValidationError("request", "invalid_request"));
        var encoded = TemplateMutationJournal.Encode(operation, requestId, actor, intent);
        var journal = new TemplateMutationJournal(db);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({encoded.LockKey})", ct);
            var replay = await journal.ReadReplayAsync(operation, requestId, actor, encoded, ct);
            if (replay is not null) return replay;
            var result = await work(new TemplateWriteSession(db), ct);
            if (!result.IsSuccess) return result;
            journal.Append(operation, requestId, actor, encoded, result.Value);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            committed = true;
            return result;
        }
        catch (CustomSewingRevisionException) { return Result.Conflict("revision_conflict"); }
        catch (CustomSewingValidationException ex) { return Result.Invalid(new ValidationError("template", ex.Message)); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("revision_conflict"); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result.Conflict("template_unique_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }
}
