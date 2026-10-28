namespace Crm.Application.CustomSewing.GetSuggestions.Contracts;

public sealed record SewingSuggestions(string ProductType, string Season, int SampleSize,
    IReadOnlyList<SuggestionNorm> Norms, bool SourceLimitReached);
