using Crm.Infrastructure.Persistence.Customers.Idempotency;
using Crm.Infrastructure.Persistence;
using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Access;
using Ardalis.Result;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace Crm.Infrastructure.Persistence.Customers.Writing;

public sealed class CustomerMutationStore(CrmDbContext db) : ICustomerMutationStore
{

    public async Task<Result<CustomerDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomerWriteSession, CancellationToken, Task<Result<CustomerDetail>>> work, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || requestId == Guid.Empty) return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        var encoded = CustomerMutationJournal.Encode(operation, requestId, actor, intent);
        var journal = new CustomerMutationJournal(db);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({encoded.LockKey})", cancellationToken);
            var replay = await journal.ReadReplayAsync(operation, requestId, actor, encoded, cancellationToken);
            if (replay is not null) return replay;
            var result = await work(new CustomerWriteSession(db), cancellationToken);
            if (!result.IsSuccess) return result;
            journal.Append(operation, requestId, actor, encoded, result.Value);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committed = true;
            return result;
        }
        catch (CustomerRevisionException) { return Result.Conflict("revision_conflict"); }
        catch (CustomerValidationException exception) { return Result.Invalid(new ValidationError("customer", exception.Message)); }
        catch (DbUpdateConcurrencyException) { return Result.Conflict("revision_conflict"); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result.Conflict("external_identity_or_primary_conflict"); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }
}
