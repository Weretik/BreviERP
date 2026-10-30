using Crm.Application.CustomSewing.GetSuggestions.Contracts;
namespace Crm.Application.CustomSewing.GetSuggestions;

public interface ISewingSuggestionSource
{
    Task<SuggestionSource> ReadAsync(string productType, string season, CancellationToken ct);
}
