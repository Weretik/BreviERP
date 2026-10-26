using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Models;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace Crm.Infrastructure.Persistence;

public sealed class CustomerMutationStore(CrmDbContext db) : ICustomerMutationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<CustomerDetail>> ExecuteAsync(string operation, Guid requestId, CrmActor actor, object intent,
        Func<ICustomerWriteSession, CancellationToken, Task<Result<CustomerDetail>>> work, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || requestId == Guid.Empty) return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        var json = JsonSerializer.Serialize(intent, intent.GetType(), JsonOptions);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        var lockKey = BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes($"{actor.UserId}:{operation}:{requestId}")));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var committed = false;
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", cancellationToken);
            var previous = await db.MutationRequests.AsNoTracking().SingleOrDefaultAsync(x => x.ActorId == actor.UserId && x.Operation == operation && x.RequestId == requestId, cancellationToken);
            if (previous is not null)
            {
                if (previous.PayloadHash != hash) return Result.Conflict("idempotency_conflict");
                var accessible = await db.Customers.AsNoTracking().AnyAsync(x => x.PublicId == previous.CustomerPublicId && x.MergedIntoId == null
                    && (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)), cancellationToken);
                if (!accessible) return Result.NotFound();
                return Result.Success(JsonSerializer.Deserialize<CustomerDetail>(previous.ResponseJson, JsonOptions)!);
            }
            var result = await work(new CustomerWriteSession(db), cancellationToken);
            if (!result.IsSuccess) return result;
            db.MutationRequests.Add(new CustomerMutationRecord
            {
                ActorId = actor.UserId, Operation = operation, RequestId = requestId, PayloadHash = hash,
                CustomerPublicId = result.Value.PublicId, IntentJson = json,
                ResponseJson = JsonSerializer.Serialize(result.Value, JsonOptions), CreatedAt = DateTimeOffset.UtcNow
            });
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
