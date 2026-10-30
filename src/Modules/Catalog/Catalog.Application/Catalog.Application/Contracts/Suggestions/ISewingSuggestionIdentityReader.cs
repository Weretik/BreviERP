namespace Catalog.Application.Contracts.Suggestions;

public interface ISewingSuggestionIdentityReader
{
    Task<SewingSuggestionIdentityBatch> ReadAsync(string sourceSeason, CancellationToken ct);
}
