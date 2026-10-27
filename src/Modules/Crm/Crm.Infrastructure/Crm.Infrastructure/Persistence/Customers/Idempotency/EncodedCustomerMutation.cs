namespace Crm.Infrastructure.Persistence.Customers.Idempotency;

internal sealed record EncodedCustomerMutation(string Json, string Hash, long LockKey);
