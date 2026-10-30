namespace Catalog.Application.Contracts.Suggestions;

public sealed record SewingSuggestionIdentity(int ProductId, long ArticleId, string ModelCode, string Name);
