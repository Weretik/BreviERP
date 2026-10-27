using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Customers.Contracts.Responses;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Customers.Idempotency;

internal sealed class CustomerMutationJournal(CrmDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static EncodedCustomerMutation Encode(string operation, Guid requestId, CrmActor actor, object intent)
    {
        var json = JsonSerializer.Serialize(intent, intent.GetType(), JsonOptions);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        var lockKey = BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes($"{actor.UserId}:{operation}:{requestId}")));
        return new EncodedCustomerMutation(json, hash, lockKey);
    }

    public async Task<Result<CustomerDetail>?> ReadReplayAsync(string operation, Guid requestId, CrmActor actor,
        EncodedCustomerMutation encoded, CancellationToken cancellationToken)
    {
        var previous = await db.MutationRequests.AsNoTracking().SingleOrDefaultAsync(x =>
            x.ActorId == actor.UserId && x.Operation == operation && x.RequestId == requestId, cancellationToken);
        if (previous is null) return null;
        if (previous.PayloadHash != encoded.Hash) return Result.Conflict("idempotency_conflict");
        var accessible = await db.Customers.AsNoTracking().AnyAsync(x =>
            x.PublicId == previous.CustomerPublicId && x.MergedIntoId == null
            && (actor.IsAdmin || x.OwnerId == new CrmUserId(actor.UserId)), cancellationToken);
        if (!accessible) return Result.NotFound();
        return Result.Success(JsonSerializer.Deserialize<CustomerDetail>(previous.ResponseJson, JsonOptions)!);
    }

    public void Append(string operation, Guid requestId, CrmActor actor, EncodedCustomerMutation encoded, CustomerDetail response)
    {
        db.MutationRequests.Add(new CustomerMutationRecord
        {
            ActorId = actor.UserId,
            Operation = operation,
            RequestId = requestId,
            PayloadHash = encoded.Hash,
            CustomerPublicId = response.PublicId,
            IntentJson = encoded.Json,
            ResponseJson = JsonSerializer.Serialize(response, JsonOptions),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }
}
