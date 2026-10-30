namespace Catalog.Application.Contracts.Suggestions;

public sealed record SewingSuggestionIdentityBatch(IReadOnlyList<SewingSuggestionIdentity> Identities, bool LimitReached);
