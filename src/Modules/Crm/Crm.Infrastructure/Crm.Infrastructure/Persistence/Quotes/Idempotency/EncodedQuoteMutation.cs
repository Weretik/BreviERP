namespace Crm.Infrastructure.Persistence.Quotes.Idempotency;

internal sealed record EncodedQuoteMutation(string Hash, long LockKey);
