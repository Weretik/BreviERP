namespace Crm.Infrastructure.Persistence.Templates.Idempotency;

internal sealed record EncodedTemplateMutation(string Hash, long LockKey);
