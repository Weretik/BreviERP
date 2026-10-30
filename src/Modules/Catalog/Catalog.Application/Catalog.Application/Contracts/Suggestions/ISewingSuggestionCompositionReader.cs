namespace Catalog.Application.Contracts.Suggestions;

public interface ISewingSuggestionCompositionReader
{
    Task<SewingSuggestionCompositionBatch> ReadAsync(IReadOnlyCollection<SewingSuggestionIdentity> identities,
        string sourceSeason, CancellationToken ct);
}
