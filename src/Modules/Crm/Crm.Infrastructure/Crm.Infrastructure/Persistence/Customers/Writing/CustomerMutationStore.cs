using BuildingBlocks.Application.Errors;
using Crm.Infrastructure.Persistence.Customers.Idempotency;
using Crm.Infrastructure.Persistence;
using Ardalis.Result;
using Crm.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Crm.Application.Abstractions.Customers;
namespace Crm.Infrastructure.Persistence.Customers.Writing;

public sealed class CustomerMutationStore(CrmDbContext db) : ICustomerMutationStore
{

    public async Task<Result<CustomerDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomerWriteSession, CancellationToken, Task<Result<CustomerDetail>>> work, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || requestId == Guid.Empty) return Result.Invalid(new ValidationError("requestId", "Неправильний запит. Оновіть сторінку й спробуйте ще раз."));
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
        catch (DbUpdateConcurrencyException) { return CustomerErrors.Stale().ToResult<CustomerDetail>(); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Result.Conflict("Такий клієнт або основний контакт уже є."); }
        finally
        {
            if (!committed) await transaction.RollbackAsync(CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }
}
