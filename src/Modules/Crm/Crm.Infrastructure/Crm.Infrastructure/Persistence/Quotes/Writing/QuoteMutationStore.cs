using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes;
using Crm.Infrastructure.Persistence.Quotes.Idempotency;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Quotes.Writing;

public sealed class QuoteMutationStore(CrmDbContext db) : IQuoteMutationStore
{
    public async Task<Result<QuoteDetail>> ExecuteAsync(
        string operation, Guid requestId, CrmActor actor, object intent,
        Func<IQuoteWriteSession, CancellationToken, Task<Result<QuoteDetail>>> work,
        CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || requestId == Guid.Empty)
            return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        var encoded = QuoteMutationJournal.Encode(operation, requestId, actor, intent);
        var journal = new QuoteMutationJournal(db);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock({encoded.LockKey})", cancellationToken);
            var replay = await journal.ReadReplayAsync(operation, requestId, actor, encoded, cancellationToken);
            if (replay is not null) return replay;
            var result = await work(new QuoteWriteSession(db), cancellationToken);
            if (!result.IsSuccess) return result;
            journal.Append(operation, requestId, actor, encoded, result.Value);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committed = true;
            return result;
        }
        catch (QuoteRevisionException) { return Result.Conflict("revision_conflict"); }
        catch (QuoteValidationException exception)
        { return Result.Invalid(new ValidationError("quote", exception.Message)); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("revision_conflict"); }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result.Conflict("quote_unique_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }
}
