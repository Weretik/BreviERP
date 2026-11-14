namespace Crm.Infrastructure.Persistence.Customers;

internal sealed record EncodedCustomerMutation(string Json, string Hash, long LockKey);
