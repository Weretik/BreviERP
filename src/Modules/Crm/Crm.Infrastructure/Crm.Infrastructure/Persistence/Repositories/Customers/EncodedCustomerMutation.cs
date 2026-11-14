
namespace Crm.Infrastructure.Persistence.Repositories.Customers;

internal sealed record EncodedCustomerMutation(string Json, string Hash, long LockKey);
