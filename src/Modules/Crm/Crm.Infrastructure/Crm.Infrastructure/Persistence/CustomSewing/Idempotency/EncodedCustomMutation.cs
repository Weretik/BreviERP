namespace Crm.Infrastructure.Persistence.CustomSewing.Idempotency;

internal sealed record EncodedCustomMutation(string Hash, long LockKey);
